using System.Security.Claims;
using BankoApi.Controllers.Shared;
using BankoApi.Controllers.Users;
using BankoApi.Controllers.Users.Requests;
using BankoApi.Controllers.Users.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.AspNetCore.Http;
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

    // GDPR endpoint tests

    private UsersController CreateAuthenticatedController(BankoDbContext ctx, Guid userId)
    {
        var controller = CreateController(ctx);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                }, "test"))
            }
        };
        return controller;
    }

    private Guid SeedUser(BankoDbContext ctx)
    {
        var userId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = "gdpr@example.com",
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("password12345", 12),
            FullName = "GDPR User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        ctx.SaveChanges();
        return userId;
    }

    [Fact]
    public void GetProfile_ReturnsProfile()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.GetProfile();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var profile = Assert.IsType<UserProfileResponse>(okResult.Value);
        Assert.Equal(userId, profile.AccountId);
        Assert.Equal("gdpr@example.com", profile.Email);
        Assert.Equal("GDPR User", profile.FullName);
    }

    [Fact]
    public void GetProfile_UserNotFound_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateAuthenticatedController(ctx, Guid.NewGuid());

        var result = controller.GetProfile();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void UpdateProfile_UpdatesFields()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.UpdateProfile(new UpdateProfileRequest
        {
            FullName = "Updated Name",
            PhoneNumber = "123456789",
            Address = "New Address"
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var profile = Assert.IsType<UserProfileResponse>(okResult.Value);
        Assert.Equal("Updated Name", profile.FullName);
        Assert.Equal("123456789", profile.PhoneNumber);
        Assert.Equal("New Address", profile.Address);
    }

    [Fact]
    public void UpdateProfile_PartialUpdate_OnlyChangesProvidedFields()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.UpdateProfile(new UpdateProfileRequest
        {
            FullName = "Only Name Changed"
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var profile = Assert.IsType<UserProfileResponse>(okResult.Value);
        Assert.Equal("Only Name Changed", profile.FullName);
        Assert.Null(profile.PhoneNumber);
    }

    [Fact]
    public void UpdateProfile_UserNotFound_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateAuthenticatedController(ctx, Guid.NewGuid());

        var result = controller.UpdateProfile(new UpdateProfileRequest
        {
            FullName = "Ghost"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void ChangePassword_Valid_ReturnsOk()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.ChangePassword(new ChangePasswordRequest
        {
            CurrentPassword = "password12345",
            NewPassword = "newpassword12345"
        });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void ChangePassword_WrongCurrent_ReturnsUnauthorized()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.ChangePassword(new ChangePasswordRequest
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword12345"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public void ChangePassword_UserNotFound_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateAuthenticatedController(ctx, Guid.NewGuid());

        var result = controller.ChangePassword(new ChangePasswordRequest
        {
            CurrentPassword = "password12345",
            NewPassword = "newpassword12345"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void AcceptConsent_Valid_ReturnsOk()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var policyId = Guid.NewGuid();
        ctx.PrivacyPolicyVersions.Add(new PrivacyPolicyVersion
        {
            Id = policyId,
            Version = 1,
            Title = "Privacy Policy v1",
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
        });
        ctx.SaveChanges();
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.AcceptConsent(new AcceptConsentRequest
        {
            PolicyVersionId = policyId
        });

        Assert.IsType<OkResult>(result);

        var user = ctx.Users.Find(userId);
        Assert.True(user!.ConsentGiven);
        Assert.Equal(policyId, user.ConsentVersionId);
        Assert.NotNull(user.ConsentUpdatedAt);

        var log = ctx.ConsentLogs.FirstOrDefault(cl => cl.UserId == userId);
        Assert.NotNull(log);
        Assert.True(log.Accepted);
    }

    [Fact]
    public void AcceptConsent_PolicyNotFound_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.AcceptConsent(new AcceptConsentRequest
        {
            PolicyVersionId = Guid.NewGuid()
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void ExportData_ReturnsData()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.ExportData();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void ExportData_WithConsentLogs_ReturnsLogs()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var policyId = Guid.NewGuid();
        ctx.PrivacyPolicyVersions.Add(new PrivacyPolicyVersion
        {
            Id = policyId,
            Version = 1,
            Title = "Privacy Policy v1",
            PublishedAt = DateTime.UtcNow,
            IsActive = true,
        });
        ctx.SaveChanges();
        var controller = CreateAuthenticatedController(ctx, userId);

        controller.AcceptConsent(new AcceptConsentRequest { PolicyVersionId = policyId });
        var result = controller.ExportData();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value, options);
        Assert.Contains("consentLogs", json);
        Assert.Contains("Privacy Policy v1", json);
    }

    [Fact]
    public void ExportData_UserNotFound_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateAuthenticatedController(ctx, Guid.NewGuid());

        var result = controller.ExportData();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void DeleteAccount_ReturnsOk()
    {
        using var ctx = CreateContext();
        var userId = SeedUser(ctx);
        var controller = CreateAuthenticatedController(ctx, userId);

        var result = controller.DeleteAccount();

        Assert.IsType<OkResult>(result);

        var user = ctx.Users.Find(userId);
        Assert.NotNull(user);
        Assert.False(user!.IsActive);
        Assert.Contains("deleted-", user.Email);
        Assert.NotNull(user.DeletedAt);
    }

    [Fact]
    public void DeleteAccount_UserNotFound_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateAuthenticatedController(ctx, Guid.NewGuid());

        var result = controller.DeleteAccount();

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
