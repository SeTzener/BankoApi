using BankoApi.Controllers.Shared.DTO;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.User;
using BankoApi.Repository;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Tests.Repository;

public class UserRepositoryTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    [Fact]
    public void CreateAccount_NewUser_ReturnsUserIdAndAddsUser()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository();
        var dto = new UserDto
        {
            Email = "test@example.com",
            Password = "password12345",
            FullName = "Test User",
            Address = "123 Test St",
            PhoneNumber = "555-0000",
            ConsentGiven = true
        };

        var userId = repo.CreateAccount(ctx, dto);
        ctx.SaveChanges();

        Assert.NotEqual(Guid.Empty, userId);
        var user = ctx.Users.Find(userId);
        Assert.NotNull(user);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("Test User", user.FullName);
        Assert.True(user.ConsentGiven);
    }

    [Fact]
    public void CreateAccount_ExistingInactiveUser_Reactivates()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository();
        var id = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = id,
            Email = "existing@example.com",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var dto = new UserDto
        {
            Email = "existing@example.com",
            Password = "newpassword123",
            FullName = "Reactivated User"
        };

        var userId = repo.CreateAccount(ctx, dto);

        Assert.Equal(id, userId);
        var user = ctx.Users.Find(id);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void CreateAccount_ExistingActiveUser_ThrowsEmailConflictException()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository();
        ctx.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Email = "active@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var dto = new UserDto
        {
            Email = "active@example.com",
            Password = "password12345"
        };

        Assert.Throws<EmailConflictException>(() => repo.CreateAccount(ctx, dto));
    }

    [Fact]
    public void ValidateAccount_ValidCredentials_ReturnsUserId()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository();
        var dto = new UserDto
        {
            Email = "valid@example.com",
            Password = "correctpassword123",
            ConsentGiven = true
        };
        var id = repo.CreateAccount(ctx, dto);
        ctx.SaveChanges();

        var userId = repo.ValidateAccount(ctx, "valid@example.com", "correctpassword123");

        Assert.Equal(id, userId);
        var user = ctx.Users.Find(id);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public void ValidateAccount_EmailNotFound_ThrowsEmailNotFoundException()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository();

        Assert.Throws<EmailNotFoundException>(() =>
            repo.ValidateAccount(ctx, "nonexistent@example.com", "password123"));
    }

    [Fact]
    public void ValidateAccount_InactiveUser_ThrowsInactiveUserException()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository();
        var id = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = id,
            Email = "inactive@example.com",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        Assert.Throws<InactiveUserException>(() =>
            repo.ValidateAccount(ctx, "inactive@example.com", "password123"));
    }

    [Fact]
    public void ValidateAccount_WrongPassword_ThrowsPasswordNotFoundException()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository();
        var dto = new UserDto
        {
            Email = "user@example.com",
            Password = "correctpassword123"
        };
        repo.CreateAccount(ctx, dto);
        ctx.SaveChanges();

        Assert.Throws<PasswordNotFoundException>(() =>
            repo.ValidateAccount(ctx, "user@example.com", "wrongpassword"));
    }
}
