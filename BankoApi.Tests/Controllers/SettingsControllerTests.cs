using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BankoApi.Controllers.Settings.Requests;
using BankoApi.Controllers.Settings.Responses;
using BankoApi.Controllers.Settings;
using BankoApi.Controllers.GoCardless.Requests;
using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace BankoApi.Tests.Controllers;

public class SettingsControllerTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private SettingsController CreateController(BankoDbContext ctx, GoCardlessService? goCardlessService = null, Guid? userId = null)
    {
        if (goCardlessService == null)
        {
            var handlerMock = MockHelpers.CreateHandlerWithToken();
            goCardlessService = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        }
        var controller = new SettingsController(goCardlessService, ctx, new BankAuthorizationRepository(), Mock.Of<ILogger<SettingsController>>());
        if (userId.HasValue)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
        return controller;
    }

    [Fact]
    public async Task GetAllTagsAsync_HasTags_ReturnsTags()
    {
        using var ctx = CreateContext();
        ctx.ExpenseTag.Add(new ExpenseTag
        {
            Id = "tag-1",
            Name = "Groceries",
            Color = 0xFF0000,
            isEarning = false
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var result = await controller.GetAllTagsAsync();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetExpenseTagsResponse>(okResult.Value);
        Assert.Single(response.ExpenseTags);
    }

    [Fact]
    public async Task GetAllTagsAsync_NoTags_ReturnsEmptyList()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var result = await controller.GetAllTagsAsync();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetExpenseTagsResponse>(okResult.Value);
        Assert.Empty(response.ExpenseTags);
    }

    [Fact]
    public async Task AddExpenseTagAsync_ValidTag_ReturnsOk()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);
        var tag = new ExpenseTag
        {
            Id = "new-tag",
            Name = "New Tag",
            Color = 0x00FF00,
            isEarning = false
        };

        var result = await controller.AddExpenseTagAsync(tag);

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(ctx.ExpenseTag);
    }

    [Fact]
    public async Task UpdateExpenseTagAsync_ExistingTag_UpdatesFields()
    {
        using var ctx = CreateContext();
        ctx.ExpenseTag.Add(new ExpenseTag
        {
            Id = "tag-update",
            Name = "Old Name",
            Color = 0x000000,
            isEarning = false
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var updatedTag = new ExpenseTag
        {
            Id = "tag-update",
            Name = "New Name",
            Color = 0xFFFFFF,
            isEarning = true,
            Aka = new List<string> { "Alias" }
        };

        var result = await controller.UpdateExpenseTagAsync("tag-update", updatedTag);

        Assert.IsType<OkObjectResult>(result);
        var tag = ctx.ExpenseTag.Find("tag-update");
        Assert.Equal("New Name", tag!.Name);
        Assert.True(tag.isEarning);
    }

    [Fact]
    public async Task UpdateExpenseTagAsync_NonExistingTag_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);
        var updatedTag = new ExpenseTag
        {
            Id = "nonexistent",
            Name = "Nope",
            Color = 0,
            isEarning = false
        };

        var result = await controller.UpdateExpenseTagAsync("nonexistent", updatedTag);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteExpenseTagAsync_ExistingTag_RemovesTag()
    {
        using var ctx = CreateContext();
        ctx.ExpenseTag.Add(new ExpenseTag
        {
            Id = "tag-delete",
            Name = "Delete Me",
            Color = 0,
            isEarning = false
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var result = await controller.DeleteExpenseTagAsync("tag-delete");

        Assert.IsType<OkObjectResult>(result);
        Assert.Empty(ctx.ExpenseTag);
    }

    [Fact]
    public async Task DeleteExpenseTagAsync_NonExistingTag_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var result = await controller.DeleteExpenseTagAsync("nonexistent");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetBankAuthorization_ExistingUser_ReturnsAuthorizations()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = BankAuthorizationStaus.Linked,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx, userId: userId);
        var result = await controller.GetBankAuthorization();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetBankAuthorizationsResponse>(okResult.Value);
        Assert.Single(response.BankAuthorizations);
    }

    [Fact]
    public async Task GetBankAuthorization_NoAuthorizations_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx, userId: Guid.NewGuid());

        var result = await controller.GetBankAuthorization();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpsertBankAuthorization_InvalidUser_ReturnsUnauthorized()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);
        var request = new UpsertBankAuthorizationRequest
        {
            UserId = Guid.NewGuid(),
            Status = BankAuthorizationStaus.Processing,
            AgreementId = "agreement-1"
        };

        var result = await controller.UpsertBankAuthorization(request);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task UpsertBankAuthorization_NewAuthorization_CreatesIt()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var request = new UpsertBankAuthorizationRequest
        {
            UserId = userId,
            Status = BankAuthorizationStaus.Processing,
            AgreementId = "agreement-new",
            InstitutionId = "TEST_BANK",
            InstitutionName = "Test Bank"
        };

        var result = await controller.UpsertBankAuthorization(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UpsertBankAuthorizationResponse>(okResult.Value);
        Assert.Equal("agreement-new", response.AgreementId);
        Assert.Equal("TEST_BANK", response.InstitutionId);
    }

    [Fact]
    public async Task UpsertBankAuthorization_ExistingAuthorization_UpdatesIt()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var authId = Guid.NewGuid();
        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = authId,
            UserId = userId,
            AgreementId = "agreement-existing",
            Status = BankAuthorizationStaus.Processing,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var request = new UpsertBankAuthorizationRequest
        {
            UserId = userId,
            Status = BankAuthorizationStaus.Linked,
            AgreementId = "agreement-existing",
            RequisitionId = "req-1"
        };

        var result = await controller.UpsertBankAuthorization(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UpsertBankAuthorizationResponse>(okResult.Value);
        Assert.Equal(BankAuthorizationStaus.Linked, response.Status);
        Assert.Equal("req-1", response.RequisitionId);
    }

    [Fact]
    public async Task GetBankAccount_ValidIds_ReturnsAccounts()
    {
        using var ctx = CreateContext();
        var bankAuthId = Guid.NewGuid();
        ctx.BankAccounts.Add(new BankAccount
        {
            Id = Guid.NewGuid(),
            BankAuthorizationId = bankAuthId,
            BankAccountId = "acc-1",
            Iban = "GB33BUKB20201555555555",
            Currency = "GBP",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var result = await controller.GetBankAccount(new List<string> { "acc-1" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetBankAccountsResponse>(okResult.Value);
        Assert.Single(response.Accounts);
    }

    [Fact]
    public async Task GetBankAccount_EmptyIds_ReturnsBadRequest()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var result = await controller.GetBankAccount(new List<string>());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetBankAccount_NonExistingId_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var result = await controller.GetBankAccount(new List<string> { "nonexistent" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpsertBankAccount_NewAccount_CreatesIt()
    {
        using var ctx = CreateContext();
        var bankAuthId = Guid.NewGuid();
        var controller = CreateController(ctx);
        var request = new UpsertBankAccountRequest
        {
            BankAuthorizationId = bankAuthId,
            BankAccountId = "new-acc",
            Iban = "DE89370400440532013000",
            Currency = "EUR",
            OwnerName = "Test Owner"
        };

        var result = await controller.UpsertBankAccount(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UpsertBankAccountResponse>(okResult.Value);
        Assert.Equal("new-acc", response.BankAccountId);
        Assert.Equal("DE89370400440532013000", response.Iban);
    }

    [Fact]
    public async Task UpsertBankAccount_ExistingAccount_UpdatesIt()
    {
        using var ctx = CreateContext();
        var bankAuthId = Guid.NewGuid();
        ctx.BankAccounts.Add(new BankAccount
        {
            Id = Guid.NewGuid(),
            BankAuthorizationId = bankAuthId,
            BankAccountId = "existing-acc",
            Iban = "OLDIBAN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var controller = CreateController(ctx);
        var request = new UpsertBankAccountRequest
        {
            BankAuthorizationId = bankAuthId,
            BankAccountId = "existing-acc",
            Iban = "NEWIBAN",
            OwnerName = "New Owner"
        };

        var result = await controller.UpsertBankAccount(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UpsertBankAccountResponse>(okResult.Value);
        Assert.Equal("NEWIBAN", response.Iban);
    }

    [Fact]
    public async Task UpsertEndUserAgreement_WithInstitutionId_CreatesNewEuaAndRequisition()
    {
        using var ctx = CreateContext();

        var handlerMock = MockHelpers.CreateHandlerWithToken();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("agreements/enduser/") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        count = 1,
                        results = new[]
                        {
                            new
                            {
                                id = "existing-eua",
                                created = DateTime.UtcNow.AddDays(-1),
                                institution_id = "TEST_BANK",
                                max_historical_days = 30,
                                access_valid_for_days = 30,
                                access_scope = new[] { "balances", "transactions" },
                                accepted = DateTime.UtcNow,
                                reconfirmation = false
                            }
                        }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("agreements/enduser/") && r.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        id = "new-eua",
                        created = DateTime.UtcNow,
                        institution_id = "TEST_BANK",
                        max_historical_days = 30,
                        access_valid_for_days = 30,
                        access_scope = new[] { "balances", "transactions" },
                        accepted = (DateTime?)null,
                        reconfirmation = false
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("requisitions/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        id = "new-req",
                        created = DateTime.UtcNow,
                        redirect = "Banko://bank-auth-callback",
                        status = "CR",
                        institution_id = "TEST_BANK",
                        agreement = "new-eua",
                        reference = "ref-123",
                        accounts = Array.Empty<string>(),
                        user_language = "EN",
                        link = "https://bank.link/req",
                        ssn = "",
                        account_selection = false,
                        redirect_immediate = false
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("institutions/TEST_BANK/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        id = "TEST_BANK",
                        name = "Test Bank",
                        bic = "TESTBIC22",
                        transaction_total_days = "180",
                        countries = new[] { "GB" },
                        logo = "https://example.com/logo.png",
                        max_access_valid_for_days = "90"
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var controller = new SettingsController(service, ctx, new BankAuthorizationRepository(), Mock.Of<ILogger<SettingsController>>());
        var request = new UpsertEndUserAgreementRequest
        {
            InstitutionId = "TEST_BANK",
            DaysOfAccess = 30
        };

        var result = await controller.UpsertEndUserAgreement(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UpsertEndUserAgreementResponse>(okResult.Value);
        Assert.Equal("new-eua", response.AgreementId);
        Assert.Equal("https://bank.link/req", response.Link);
    }

    [Fact]
    public async Task UpsertEndUserAgreement_WithValidExistingEua_UsesExisting()
    {
        using var ctx = CreateContext();

        var handlerMock = MockHelpers.CreateHandlerWithToken();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("agreements/enduser/") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        count = 1,
                        results = new[]
                        {
                            new
                            {
                                id = "valid-eua",
                                created = DateTime.UtcNow,
                                institution_id = "TEST_BANK",
                                max_historical_days = 30,
                                access_valid_for_days = 30,
                                access_scope = new[] { "balances", "transactions" },
                                accepted = (DateTime?)null,
                                reconfirmation = false
                            }
                        }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("requisitions/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        id = "req-1",
                        created = DateTime.UtcNow,
                        redirect = "Banko://bank-auth-callback",
                        status = "CR",
                        institution_id = "TEST_BANK",
                        agreement = "valid-eua",
                        reference = "ref-1",
                        accounts = Array.Empty<string>(),
                        user_language = "EN",
                        link = "https://bank.link/req",
                        ssn = "",
                        account_selection = false,
                        redirect_immediate = false
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("institutions/TEST_BANK/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        id = "TEST_BANK",
                        name = "Test Bank",
                        bic = "TESTBIC22",
                        transaction_total_days = "180",
                        countries = new[] { "GB" },
                        logo = "https://example.com/logo.png",
                        max_access_valid_for_days = "90"
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var controller = new SettingsController(service, ctx, new BankAuthorizationRepository(), Mock.Of<ILogger<SettingsController>>());
        var request = new UpsertEndUserAgreementRequest();

        var result = await controller.UpsertEndUserAgreement(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UpsertEndUserAgreementResponse>(okResult.Value);
        Assert.Equal("valid-eua", response.AgreementId);
    }
}
