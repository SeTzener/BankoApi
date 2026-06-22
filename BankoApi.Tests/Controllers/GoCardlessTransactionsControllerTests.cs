using System.Net;
using System.Text.Json;
using BankoApi.Controllers.GoCardless;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BankoApi.Tests.Controllers;

public class GoCardlessTransactionsControllerTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    [Fact]
    public async Task FetchAndStoreTransactions_NoUser_ReturnsWithNoUser()
    {
        using var ctx = CreateContext();
        var handlerMock = MockHelpers.CreateHandlerWithToken();
        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var controller = new TransactionsController(service, ctx);
        var bankAccountId = Guid.NewGuid();

        var result = await controller.FetchAndStoreTransactions(bankAccountId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task FetchAndStoreTransactions_EmptyResponse_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        // Return empty body which will deserialize as null Transactions
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
            });
        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var controller = new TransactionsController(service, ctx);
        var bankAccountId = Guid.NewGuid();

        var result = await controller.FetchAndStoreTransactions(bankAccountId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task FetchAndStoreTransactions_EndUserAgreementException_ReturnsUnauthorized()
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
        var agreementId = Guid.NewGuid().ToString();
        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AgreementId = agreementId,
            Status = BankAuthorizationStaus.Linked,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent($"EUA was valid for 90 days and it expired {agreementId}")
            });
        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var controller = new TransactionsController(service, ctx);
        var bankAccountId = Guid.NewGuid();

        var result = await controller.FetchAndStoreTransactions(bankAccountId);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task FetchAndStoreTransactions_GeneralException_ReturnsBadRequest()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        // Return 500 to cause HttpRequestException
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var controller = new TransactionsController(service, ctx);
        var bankAccountId = Guid.NewGuid();

        var result = await controller.FetchAndStoreTransactions(bankAccountId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task FetchAndStoreTransactions_Success_ReturnsOk()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var transactionsResponse = new
        {
            transactions = new
            {
                booked = Array.Empty<object>(),
                pending = Array.Empty<object>()
            }
        };

        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(transactionsResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });
        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var controller = new TransactionsController(service, ctx);
        var bankAccountId = Guid.NewGuid();

        var result = await controller.FetchAndStoreTransactions(bankAccountId);

        Assert.IsType<OkObjectResult>(result);
    }
}
