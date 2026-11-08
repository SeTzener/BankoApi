using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Services.Push;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankoApi.Tests.Services;

public class EuaExpiryNotificationServiceTests
{
    private static readonly DateTime Now = new(2026, 11, 8, 8, 0, 0, DateTimeKind.Utc);

    private static BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private static BankAuthorization Authorization(
        Guid userId,
        DateTime? expiresAt,
        BankAuthorizationStaus status = BankAuthorizationStaus.Linked,
        DateTime? warningNotifiedFor = null,
        DateTime? expiredNotifiedFor = null,
        bool accountIsActive = true)
    {
        var authorizationId = Guid.NewGuid();
        return new BankAuthorization
        {
            Id = authorizationId,
            UserId = userId,
            Status = status,
            AgreementExpiresAt = expiresAt,
            ExpiryWarningNotifiedFor = warningNotifiedFor,
            ExpiryExpiredNotifiedFor = expiredNotifiedFor,
            CreatedAt = Now,
            UpdatedAt = Now,
            BankAccounts = new List<BankAccount>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    BankAuthorizationId = authorizationId,
                    BankAccountId = Guid.NewGuid().ToString(),
                    IsActive = accountIsActive,
                    CreatedAt = Now,
                    UpdatedAt = Now
                }
            }
        };
    }

    private static PushDevice Device(Guid userId, string token = "token-1") => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Token = token,
        Platform = PushPlatform.Android,
        IsActive = true,
        CreatedAt = Now,
        UpdatedAt = Now
    };

    private static EuaExpiryNotificationService CreateService(BankoDbContext context, IPushSender sender)
    {
        var pushNotificationService = new PushNotificationService(
            context, sender, NullLogger<PushNotificationService>.Instance);
        return new EuaExpiryNotificationService(
            context, pushNotificationService, NullLogger<EuaExpiryNotificationService>.Instance);
    }

    [Fact]
    public async Task RunAsync_AgreementInsideWarningWindow_SendsWarningAndMarksExpiry()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var expiry = Now.AddDays(5);
        context.BankAuthorizations.Add(Authorization(userId, expiry));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        var call = Assert.Single(sender.Calls);
        Assert.Equal("eua_expiry_warning", call.Message.Data!["type"]);
        Assert.Equal(expiry, context.BankAuthorizations.Single().ExpiryWarningNotifiedFor);
    }

    [Fact]
    public async Task RunAsync_MultipleAgreementsForOneUser_SendsOneGroupedWarning()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var firstExpiry = Now.AddDays(3);
        var secondExpiry = Now.AddDays(6);
        context.BankAuthorizations.AddRange(
            Authorization(userId, firstExpiry),
            Authorization(userId, secondExpiry));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Single(sender.Calls);
        Assert.All(context.BankAuthorizations.ToList(), authorization =>
        {
            Assert.Equal(authorization.AgreementExpiresAt, authorization.ExpiryWarningNotifiedFor);
        });
    }

    [Fact]
    public async Task RunAsync_AlreadyWarnedForSameExpiry_DoesNotSendAgain()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var expiry = Now.AddDays(4);
        context.BankAuthorizations.Add(Authorization(userId, expiry, warningNotifiedFor: expiry));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Empty(sender.Calls);
    }

    [Fact]
    public async Task RunAsync_RenewedAgreementWithNewExpiry_SendsAnotherWarning()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var previousExpiry = Now.AddDays(-30);
        var renewedExpiry = Now.AddDays(4);
        context.BankAuthorizations.Add(
            Authorization(userId, renewedExpiry, warningNotifiedFor: previousExpiry));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Single(sender.Calls);
        Assert.Equal(renewedExpiry, context.BankAuthorizations.Single().ExpiryWarningNotifiedFor);
    }

    [Fact]
    public async Task RunAsync_ExpiredAgreement_SendsExpiredNotification()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var expiry = Now.AddDays(-1);
        context.BankAuthorizations.Add(Authorization(userId, expiry));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        var call = Assert.Single(sender.Calls);
        Assert.Equal("eua_expiry_expired", call.Message.Data!["type"]);
        Assert.Equal(expiry, context.BankAuthorizations.Single().ExpiryExpiredNotifiedFor);
    }

    [Fact]
    public async Task RunAsync_ExpiredStatus_SendsExpiredNotification()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var expiry = Now.AddDays(-3);
        context.BankAuthorizations.Add(
            Authorization(userId, expiry, status: BankAuthorizationStaus.Expired));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        var call = Assert.Single(sender.Calls);
        Assert.Equal("eua_expiry_expired", call.Message.Data!["type"]);
    }

    [Fact]
    public async Task RunAsync_AlreadyNotifiedAsExpired_DoesNotSendAgain()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var expiry = Now.AddDays(-2);
        context.BankAuthorizations.Add(
            Authorization(userId, expiry, expiredNotifiedFor: expiry));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Empty(sender.Calls);
    }

    [Fact]
    public async Task RunAsync_NoActiveAccounts_DoesNotSend()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.BankAuthorizations.Add(Authorization(userId, Now.AddDays(3), accountIsActive: false));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Empty(sender.Calls);
    }

    [Fact]
    public async Task RunAsync_NoRegisteredDevice_DoesNotMarkAgreementAsNotified()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.BankAuthorizations.Add(Authorization(userId, Now.AddDays(3)));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Empty(sender.Calls);
        Assert.Null(context.BankAuthorizations.Single().ExpiryWarningNotifiedFor);
    }

    [Fact]
    public async Task RunAsync_PushFails_DoesNotMarkAgreementAsNotified()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.BankAuthorizations.Add(Authorization(userId, Now.AddDays(3)));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender { StatusFor = _ => PushSendStatus.Failed };
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Single(sender.Calls);
        Assert.Null(context.BankAuthorizations.Single().ExpiryWarningNotifiedFor);
    }

    [Fact]
    public async Task RunAsync_OutsideWarningWindow_DoesNotSend()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.BankAuthorizations.Add(Authorization(userId, Now.AddDays(30)));
        context.PushDevices.Add(Device(userId));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.RunAsync(Now);

        Assert.Empty(sender.Calls);
    }

    private sealed class FakePushSender : IPushSender
    {
        public List<(IReadOnlyList<string> Tokens, PushMessage Message)> Calls { get; } = new();

        public Func<string, PushSendStatus> StatusFor { get; init; } = _ => PushSendStatus.Sent;

        public Task<IReadOnlyList<PushSendResult>> SendAsync(
            IReadOnlyList<string> tokens,
            PushMessage message,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((tokens, message));
            IReadOnlyList<PushSendResult> results = tokens
                .Select(token => new PushSendResult(token, StatusFor(token)))
                .ToList();
            return Task.FromResult(results);
        }
    }
}
