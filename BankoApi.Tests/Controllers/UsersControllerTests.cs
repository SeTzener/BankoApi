using BankoApi.Controllers.Users;
using BankoApi.Controllers.Users.Requests;
using BankoApi.Controllers.Users.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankoApi.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<TokenService> _tokenServiceMock = new(MockBehavior.Default, null!, null!);
    private TokenService _tokenService => _tokenServiceMock.Object;

    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private UsersController CreateController(BankoDbContext ctx)
    {
        _tokenServiceMock
            .Setup(t => t.GenerateTokens(It.IsAny<User>()))
            .Returns(("test-access-token", "test-refresh-token", 900L));

        return new UsersController(ctx, new UserRepository(), _tokenService, Mock.Of<ILogger<UsersController>>());
    }

    [Fact]
    public async Task NewUser_ValidRequest_ReturnsOkWithUserId()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var request = new NewUserRequest
        {
            Email = "new@example.com",
            Password = "password12345",
            FullName = "New User",
            ConsentGiven = true
        };

        var result = await controller.NewUser(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task NewUser_DuplicateEmail_ReturnsConflict()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Email = "existing@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var request = new NewUserRequest
        {
            Email = "existing@example.com",
            Password = "password12345"
        };

        var result = await controller.NewUser(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        // Create user first via the controller
        var createRequest = new NewUserRequest
        {
            Email = "login@example.com",
            Password = "correctpassword123"
        };
        await controller.NewUser(createRequest);

        // Now login
        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "correctpassword123"
        };

        var result = await controller.Login(loginRequest);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsRealTokens()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var createRequest = new NewUserRequest
        {
            Email = "token-test@example.com",
            Password = "password12345"
        };
        var createResult = await controller.NewUser(createRequest);
        var createOk = Assert.IsType<OkObjectResult>(createResult);
        var createResponse = Assert.IsType<UserResponse>(createOk.Value);

        Assert.Equal("test-access-token", createResponse.AccessToken);
        Assert.Equal("test-refresh-token", createResponse.RefreshToken);
        Assert.Equal(900L, createResponse.ExpiresIn);
        Assert.NotEqual(Guid.Empty, createResponse.AccountId);

        var loginResult = await controller.Login(new LoginRequest
        {
            Email = "token-test@example.com",
            Password = "password12345"
        });
        var loginOk = Assert.IsType<OkObjectResult>(loginResult);
        var loginResponse = Assert.IsType<UserResponse>(loginOk.Value);

        Assert.Equal("test-access-token", loginResponse.AccessToken);
        Assert.Equal("test-refresh-token", loginResponse.RefreshToken);
        Assert.Equal(900L, loginResponse.ExpiresIn);
        Assert.NotEqual(Guid.Empty, loginResponse.AccountId);
    }

    [Fact]
    public async Task Login_WrongCredentials_ReturnsUnauthorized()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var createRequest = new NewUserRequest
        {
            Email = "login2@example.com",
            Password = "correctpassword123"
        };
        await controller.NewUser(createRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login2@example.com",
            Password = "wrongpassword"
        };

        var result = await controller.Login(loginRequest);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsForbidden()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = "inactive@example.com",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var loginRequest = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "anypassword"
        };

        var result = await controller.Login(loginRequest);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task NewUser_MinimalData_ReturnsOk()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var request = new NewUserRequest
        {
            Email = "minimal@example.com",
            Password = "minimalpassword"
        };

        var result = await controller.NewUser(request);

        Assert.IsType<OkObjectResult>(result);
    }
}
