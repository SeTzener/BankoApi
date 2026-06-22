using BankoApi.Controllers.Users;
using BankoApi.Controllers.Users.Requests;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankoApi.Tests.Controllers;

public class UsersControllerTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    [Fact]
    public async Task NewUser_ValidRequest_ReturnsOkWithUserId()
    {
        using var ctx = CreateContext();
        var controller = new UsersController(ctx, new UserRepository(), Mock.Of<ILogger<UsersController>>());

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
        var userId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = "existing@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = new UsersController(ctx, new UserRepository(), Mock.Of<ILogger<UsersController>>());
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
        var controller = new UsersController(ctx, new UserRepository(), Mock.Of<ILogger<UsersController>>());

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
    public async Task Login_WrongCredentials_ReturnsUnauthorized()
    {
        using var ctx = CreateContext();
        var controller = new UsersController(ctx, new UserRepository(), Mock.Of<ILogger<UsersController>>());

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

        var controller = new UsersController(ctx, new UserRepository(), Mock.Of<ILogger<UsersController>>());
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
        var controller = new UsersController(ctx, new UserRepository(), Mock.Of<ILogger<UsersController>>());

        var request = new NewUserRequest
        {
            Email = "minimal@example.com",
            Password = "minimalpassword"
        };

        var result = await controller.NewUser(request);

        Assert.IsType<OkObjectResult>(result);
    }
}
