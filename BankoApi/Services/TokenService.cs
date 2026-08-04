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

    public virtual async Task<(string accessToken, string refreshToken, long expiresIn)> GenerateTokensAsync(User user)
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

        return (accessToken, refreshToken, expiresIn);
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

        if (storedToken.IsUsed)
        {
            RevokeSingleToken(storedToken);
            throw new SecurityTokenException("Refresh token is already used — possible token reuse detected");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new SecurityTokenException("Refresh token has expired");

        return await ExecuteRefreshTransactionAsync(storedToken);
    }

    private async Task<(Guid userId, string accessToken, string refreshToken, long expiresIn)> ExecuteRefreshTransactionAsync(RefreshToken storedToken)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await TryBeginTransactionAsync();

            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            try
            {
                var (accessToken, newRefreshToken, expiresIn) = await GenerateTokensAsync(storedToken.User);
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

    private void RevokeSingleToken(RefreshToken token)
    {
        token.IsRevoked = true;
        _context.RefreshTokens.Update(token);
        _context.SaveChanges();
    }
}
