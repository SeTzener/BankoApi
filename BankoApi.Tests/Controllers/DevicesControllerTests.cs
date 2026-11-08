using System.Security.Claims;
using BankoApi.Controllers.Devices;
using BankoApi.Controllers.Devices.Requests;
using BankoApi.Controllers.Devices.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankoApi.Tests.Controllers;

public class DevicesControllerTests
{
    private static BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private static void AddUser(BankoDbContext ctx, Guid userId)
    {
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = $"user-{userId:N}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private static DevicesController CreateController(BankoDbContext ctx, Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var controller = new DevicesController(ctx, Mock.Of<ILogger<DevicesController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
        return controller;
    }

    [Fact]
    public async Task Register_NewToken_CreatesActiveDevice()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        AddUser(ctx, userId);
        var controller = CreateController(ctx, userId);

        var result = await controller.Register(new RegisterDeviceRequest
        {
            Token = "fcm-token-1",
            Platform = "Android"
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RegisterDeviceResponse>(okResult.Value);
        Assert.True(response.IsActive);
        Assert.Equal(PushPlatform.Android, response.Platform);

        var saved = Assert.Single(ctx.PushDevices);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal("fcm-token-1", saved.Token);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task Register_SameTokenTwice_UpdatesInsteadOfDuplicating()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        AddUser(ctx, userId);
        var controller = CreateController(ctx, userId);

        var request = new RegisterDeviceRequest { Token = "fcm-token-1", Platform = "Android" };
        await controller.Register(request);
        var first = ctx.PushDevices.Single();
        var firstSeen = first.LastSeenAt;

        var result = await controller.Register(request);

        Assert.IsType<OkObjectResult>(result);
        var saved = Assert.Single(ctx.PushDevices);
        Assert.Equal(first.Id, saved.Id);
        Assert.True(saved.LastSeenAt >= firstSeen);
    }

    [Fact]
    public async Task Register_TokenOwnedByAnotherUser_ReassignsToCurrentUser()
    {
        using var ctx = CreateContext();
        var previousUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        AddUser(ctx, previousUserId);
        AddUser(ctx, currentUserId);

        var deviceId = Guid.NewGuid();
        ctx.PushDevices.Add(new PushDevice
        {
            Id = deviceId,
            UserId = previousUserId,
            Token = "shared-device-token",
            Platform = PushPlatform.Ios,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var controller = CreateController(ctx, currentUserId);
        var result = await controller.Register(new RegisterDeviceRequest
        {
            Token = "shared-device-token",
            Platform = "Android"
        });

        Assert.IsType<OkObjectResult>(result);
        var saved = Assert.Single(ctx.PushDevices);
        Assert.Equal(deviceId, saved.Id);
        Assert.Equal(currentUserId, saved.UserId);
        Assert.Equal(PushPlatform.Android, saved.Platform);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task Register_MissingToken_ReturnsBadRequest()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        AddUser(ctx, userId);
        var controller = CreateController(ctx, userId);

        var result = await controller.Register(new RegisterDeviceRequest { Token = " ", Platform = "Android" });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(ctx.PushDevices);
    }

    [Fact]
    public async Task Register_InvalidPlatform_ReturnsBadRequest()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        AddUser(ctx, userId);
        var controller = CreateController(ctx, userId);

        var result = await controller.Register(new RegisterDeviceRequest { Token = "fcm-token-1", Platform = "Windows" });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(ctx.PushDevices);
    }

    [Fact]
    public async Task Unregister_OwnToken_MarksDeviceInactive()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        AddUser(ctx, userId);
        var controller = CreateController(ctx, userId);

        await controller.Register(new RegisterDeviceRequest { Token = "fcm-token-1", Platform = "Ios" });

        var result = await controller.Unregister("fcm-token-1");

        Assert.IsType<NoContentResult>(result);
        var saved = Assert.Single(ctx.PushDevices);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task Unregister_UnknownToken_ReturnsNoContent()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        AddUser(ctx, userId);
        var controller = CreateController(ctx, userId);

        var result = await controller.Unregister("unknown-token");

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(ctx.PushDevices);
    }

    [Fact]
    public async Task Unregister_AnotherUsersToken_DoesNotDeactivateIt()
    {
        using var ctx = CreateContext();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        AddUser(ctx, ownerId);
        AddUser(ctx, otherId);

        ctx.PushDevices.Add(new PushDevice
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Token = "owner-token",
            Platform = PushPlatform.Android,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var controller = CreateController(ctx, otherId);
        var result = await controller.Unregister("owner-token");

        Assert.IsType<NoContentResult>(result);
        Assert.True(Assert.Single(ctx.PushDevices).IsActive);
    }
}
