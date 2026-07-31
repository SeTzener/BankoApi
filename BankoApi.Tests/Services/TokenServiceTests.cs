using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BankoApi.Tests.Services;

public class TokenServiceTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private IConfiguration CreateConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "This-Is-A-Test-Secret-Key-That-Is-At-Least-32-Chars!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            })!
            .Build();
    }

    private User CreateUser(BankoDbContext ctx)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("password12345", 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        ctx.Users.Add(user);
        ctx.SaveChanges();
        return user;
    }

    [Fact]
    public async Task GenerateTokens_ReturnsValidTokens()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);

        var (accessToken, refreshToken, expiresIn) = await service.GenerateTokensAsync(user);

        Assert.NotEmpty(accessToken);
        Assert.NotEmpty(refreshToken);
        Assert.Equal(900L, expiresIn);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        Assert.Equal(user.UserId.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.NotEmpty(jwt.Id);
    }

    [Fact]
    public async Task GenerateTokens_CreatesRefreshTokenInDb()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);

        await service.GenerateTokensAsync(user);

        var storedToken = ctx.RefreshTokens.FirstOrDefault(rt => rt.UserId == user.UserId);
        Assert.NotNull(storedToken);
        Assert.False(storedToken.IsUsed);
        Assert.False(storedToken.IsRevoked);
        Assert.Equal(user.UserId, storedToken.UserId);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);
        var (_, originalRefresh, _) = await service.GenerateTokensAsync(user);

        var (userId, accessToken, refreshToken, expiresIn) = await service.RefreshTokenAsync(originalRefresh);

        Assert.Equal(user.UserId, userId);
        Assert.NotEmpty(accessToken);
        Assert.NotEmpty(refreshToken);
        Assert.NotEqual(originalRefresh, refreshToken);
        Assert.Equal(900L, expiresIn);

        var oldToken = ctx.RefreshTokens.FirstOrDefault(rt => rt.Token == originalRefresh);
        Assert.NotNull(oldToken);
        Assert.True(oldToken.IsUsed);
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_Throws()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);

        var expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Token = Convert.ToBase64String(new byte[64]),
            JwtId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3),
            IsUsed = false,
            IsRevoked = false,
        };
        ctx.RefreshTokens.Add(expiredToken);
        ctx.SaveChanges();

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            service.RefreshTokenAsync(expiredToken.Token));
    }

    [Fact]
    public async Task RefreshTokenAsync_UsedToken_RevokesSingleTokenOnly()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);
        var (_, firstRefresh, _) = await service.GenerateTokensAsync(user);
        var (_, secondRefresh, _) = await service.GenerateTokensAsync(user);

        var usedToken = ctx.RefreshTokens.First(rt => rt.Token == firstRefresh);
        usedToken.IsUsed = true;
        ctx.SaveChanges();

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            service.RefreshTokenAsync(firstRefresh));

        var revokedToken = ctx.RefreshTokens.First(rt => rt.Token == firstRefresh);
        Assert.True(revokedToken.IsRevoked);

        var otherToken = ctx.RefreshTokens.First(rt => rt.Token == secondRefresh);
        Assert.False(otherToken.IsRevoked);
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_Throws()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);
        var (_, refresh, _) = await service.GenerateTokensAsync(user);

        var storedToken = ctx.RefreshTokens.First(rt => rt.Token == refresh);
        storedToken.IsRevoked = true;
        ctx.SaveChanges();

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            service.RefreshTokenAsync(refresh));
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_RevokesToken()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);
        var (_, refresh, _) = await service.GenerateTokensAsync(user);

        await service.RevokeRefreshTokenAsync(refresh);

        var storedToken = ctx.RefreshTokens.First(rt => rt.Token == refresh);
        Assert.True(storedToken.IsRevoked);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_NonExistentToken_DoesNotThrow()
    {
        using var ctx = CreateContext();
        var config = CreateConfig();
        var service = new TokenService(config, ctx);

        await service.RevokeRefreshTokenAsync("nonexistent-token");
    }

    [Fact]
    public async Task GenerateTokens_DefaultConfig_UsesDefaults()
    {
        using var ctx = CreateContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "This-Is-A-Test-Secret-Key-That-Is-At-Least-32-Chars!",
            })!
            .Build();
        var service = new TokenService(config, ctx);
        var user = CreateUser(ctx);

        var (accessToken, _, expiresIn) = await service.GenerateTokensAsync(user);

        Assert.NotEmpty(accessToken);
        Assert.Equal(900L, expiresIn);
    }
}
