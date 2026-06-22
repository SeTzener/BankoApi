using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankoApi.Tests.Services;

public class ScheduledTaskServiceTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private IServiceScope CreateScope(BankoDbContext ctx, GoCardlessService goCardlessService)
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(BankoDbContext)))
            .Returns(ctx);
        serviceProviderMock.Setup(x => x.GetService(typeof(GoCardlessService)))
            .Returns(goCardlessService);
        serviceProviderMock.Setup(x => x.GetService(typeof(TransactionsRepository)))
            .Returns(new TransactionsRepository());

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        return scopeMock.Object;
    }

    [Fact]
    public async Task ExecuteAsync_NoUsers_CompletesWithoutError()
    {
        using var ctx = CreateContext();
        var handlerMock = MockHelpers.CreateHandlerWithToken();
        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var scope = CreateScope(ctx, service);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scope);

        var scheduledService = new ScheduledTaskService(scopeFactoryMock.Object, Mock.Of<ILogger<ScheduledTaskService>>());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await scheduledService.StartAsync(cts.Token);
        await Task.Delay(200);
        await scheduledService.StopAsync(cts.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WithUsersAndBankAccounts_ProcessesTransactions()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAuthId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = bankAuthId,
            UserId = userId,
            Status = BankAuthorizationStaus.Linked,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BankAccounts = new List<BankAccount>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    BankAuthorizationId = bankAuthId,
                    BankAccountId = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
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
            new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(transactionsResponse,
                        new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }))
            });

        var goCardlessService = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var scope = CreateScope(ctx, goCardlessService);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scope);

        var scheduledService = new ScheduledTaskService(scopeFactoryMock.Object, Mock.Of<ILogger<ScheduledTaskService>>());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await scheduledService.StartAsync(cts.Token);
        await Task.Delay(200);
        await scheduledService.StopAsync(cts.Token);
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionInFetch_DoesNotCrashService()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAuthId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            UserId = userId,
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = bankAuthId,
            UserId = userId,
            Status = BankAuthorizationStaus.Linked,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BankAccounts = new List<BankAccount>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    BankAuthorizationId = bankAuthId,
                    BankAccountId = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        });
        ctx.SaveChanges();

        // Return 500 to cause exception in GetTransactionsAsync
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));

        var goCardlessService = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var scope = CreateScope(ctx, goCardlessService);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scope);

        var scheduledService = new ScheduledTaskService(scopeFactoryMock.Object, Mock.Of<ILogger<ScheduledTaskService>>());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await scheduledService.StartAsync(cts.Token);
        await Task.Delay(200);
        await scheduledService.StopAsync(cts.Token);
    }
}
