using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BankoApi.Data;
using BankoApi.Data.Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;

namespace BankoApi.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly BankoDbContext _context;

    public TokenService(IConfiguration configuration, BankoDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    private const int MaxReuseChainDepth = 5;

    public virtual async Task<(string accessToken, string refreshToken, long expiresIn)> GenerateTokensAsync(User user)
    {
        var (accessToken, refreshToken, expiresIn, _) = await CreateTokenPairAsync(user);
        return (accessToken, refreshToken, expiresIn);
    }

    private async Task<(string accessToken, string refreshToken, long expiresIn, RefreshToken refreshTokenEntity)> CreateTokenPairAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiresIn = GetAccessTokenExpirationMinutes() * 60L;

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Token = refreshToken,
            JwtId = GetJwtId(accessToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return (accessToken, refreshToken, expiresIn, refreshTokenEntity);
    }

    public async Task<(Guid userId, string accessToken, string refreshToken, long expiresIn)> RefreshTokenAsync(string oldRefreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == oldRefreshToken);

        if (storedToken == null)
            throw new SecurityTokenException("Refresh token not found");

        if (storedToken.IsRevoked)
            throw new SecurityTokenException("Refresh token is revoked");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new SecurityTokenException("Refresh token has expired");

        if (storedToken.IsUsed)
            return await HandleUsedTokenAsync(storedToken);

        try
        {
            return await RotateRefreshTokenAsync(storedToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            var refreshed = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == oldRefreshToken);

            if (refreshed == null || !refreshed.IsUsed)
                throw new SecurityTokenException("Refresh token could not be rotated");

            return await HandleUsedTokenAsync(refreshed);
        }
    }

    private async Task<(Guid userId, string accessToken, string refreshToken, long expiresIn)> HandleUsedTokenAsync(RefreshToken usedToken)
    {
        if (!IsWithinReuseGrace(usedToken))
        {
            RevokeSingleToken(usedToken);
            throw new SecurityTokenException("Refresh token is already used — possible token reuse detected");
        }

        try
        {
            var newest = await ResolveNewestTokenAsync(usedToken);

            if (newest == null || newest.IsRevoked || newest.ExpiresAt < DateTime.UtcNow)
            {
                RevokeSingleToken(usedToken);
                throw new SecurityTokenException("Refresh token reuse detected outside a valid token chain");
            }

            if (newest.IsUsed && newest.Id == usedToken.Id)
            {
                RevokeSingleToken(usedToken);
                throw new SecurityTokenException("Refresh token reuse detected without a replacement token");
            }

            if (newest.IsUsed)
            {
                // Another request already rotated the chain end; hand back the current valid pair.
                var accessToken = GenerateAccessToken(newest.User);
                return (newest.UserId, accessToken, newest.Token, GetAccessTokenExpirationMinutes() * 60L);
            }

            return await RotateRefreshTokenAsync(newest);
        }
        catch (DbUpdateConcurrencyException)
        {
            var reResolved = await ResolveNewestTokenAsync(usedToken);
            if (reResolved == null || reResolved.IsUsed || reResolved.IsRevoked || reResolved.ExpiresAt < DateTime.UtcNow)
                throw new SecurityTokenException("Refresh token reuse detected");

            var accessToken = GenerateAccessToken(reResolved.User);
            return (reResolved.UserId, accessToken, reResolved.Token, GetAccessTokenExpirationMinutes() * 60L);
        }
    }

    private bool IsWithinReuseGrace(RefreshToken usedToken)
    {
        if (usedToken.UsedAt == null) return false;
        return DateTime.UtcNow - usedToken.UsedAt.Value <= TimeSpan.FromSeconds(GetRefreshReuseGraceSeconds());
    }

    private async Task<RefreshToken?> ResolveNewestTokenAsync(RefreshToken token)
    {
        var current = token;
        for (var hop = 0; hop < MaxReuseChainDepth; hop++)
        {
            if (current.ReplacedByTokenId == null) return current;

            var next = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Id == current.ReplacedByTokenId);

            if (next == null) return current;
            current = next;
        }

        return current;
    }

    private async Task<(Guid userId, string accessToken, string refreshToken, long expiresIn)> RotateRefreshTokenAsync(RefreshToken storedToken)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await TryBeginTransactionAsync();

            storedToken.IsUsed = true;
            storedToken.UsedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            try
            {
                var (accessToken, newRefreshToken, expiresIn, newRefreshTokenEntity) =
                    await CreateTokenPairAsync(storedToken.User);

                storedToken.ReplacedByTokenId = newRefreshTokenEntity.Id;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                if (transaction != null) await transaction.CommitAsync();
                return (storedToken.UserId, accessToken, newRefreshToken, expiresIn);
            }
            catch
            {
                if (transaction != null) await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task<IDbContextTransaction?> TryBeginTransactionAsync()
    {
        try
        {
            return await _context.Database.BeginTransactionAsync();
        }
        catch (InvalidOperationException)
        {
            // Transactions not supported (e.g., in-memory test database)
            return null;
        }
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateAccessToken(User user)
    {
        var secret = _configuration["Jwt:Secret"]
                     ?? throw new InvalidOperationException("JWT Secret is not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "BankoApi";
        var audience = _configuration["Jwt:Audience"] ?? "BankoMobile";
        var expirationMinutes = GetAccessTokenExpirationMinutes();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string GetJwtId(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        return jwt.Id;
    }

    private int GetAccessTokenExpirationMinutes()
    {
        return int.TryParse(_configuration["Jwt:AccessTokenExpirationMinutes"], out var minutes)
            ? minutes
            : 15;
    }

    private int GetRefreshTokenExpirationDays()
    {
        return int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out var days)
            ? days
            : 7;
    }

    private int GetRefreshReuseGraceSeconds()
    {
        return int.TryParse(_configuration["Jwt:RefreshReuseGraceSeconds"], out var seconds)
            ? seconds
            : 300;
    }

    private void RevokeSingleToken(RefreshToken token)
    {
        token.IsRevoked = true;
        _context.RefreshTokens.Update(token);
        _context.SaveChanges();
    }
}
