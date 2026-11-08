using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Services.Push;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankoApi.Tests.Services;

public class PushNotificationServiceTests
{
    private static BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private static PushDevice Device(Guid userId, string token, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Token = token,
        Platform = PushPlatform.Android,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static PushNotificationService CreateService(BankoDbContext context, IPushSender sender) =>
        new(context, sender, NullLogger<PushNotificationService>.Instance);

    [Fact]
    public async Task SendToUserAsync_ActiveDevices_SendsToEveryToken()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.PushDevices.AddRange(
            Device(userId, "token-1"),
            Device(userId, "token-2"));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        var summary = await service.SendToUserAsync(userId, new PushMessage("Title", "Body"));

        var call = Assert.Single(sender.Calls);
        Assert.Equal(new[] { "token-1", "token-2" }, call.Tokens.OrderBy(token => token));
        Assert.Equal(2, summary.Sent);
        Assert.Equal(0, summary.Failed);
        Assert.Equal(0, summary.Deactivated);
    }

    [Fact]
    public async Task SendToUserAsync_InvalidToken_DeactivatesOnlyThatDevice()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.PushDevices.AddRange(
            Device(userId, "valid-token"),
            Device(userId, "stale-token"));
        await context.SaveChangesAsync();

        var sender = new FakePushSender
        {
            StatusFor = token => token == "stale-token" ? PushSendStatus.InvalidToken : PushSendStatus.Sent
        };
        var service = CreateService(context, sender);

        var summary = await service.SendToUserAsync(userId, new PushMessage("Title", "Body"));

        Assert.Equal(1, summary.Sent);
        Assert.Equal(1, summary.Deactivated);
        Assert.True(context.PushDevices.Single(device => device.Token == "valid-token").IsActive);
        Assert.False(context.PushDevices.Single(device => device.Token == "stale-token").IsActive);
    }

    [Fact]
    public async Task SendToUserAsync_IgnoresInactiveDevicesAndOtherUsers()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        context.PushDevices.AddRange(
            Device(userId, "active-token"),
            Device(userId, "inactive-token", isActive: false),
            Device(otherUserId, "other-user-token"));
        await context.SaveChangesAsync();

        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        await service.SendToUserAsync(userId, new PushMessage("Title", "Body"));

        var call = Assert.Single(sender.Calls);
        Assert.Equal(new[] { "active-token" }, call.Tokens);
    }

    [Fact]
    public async Task SendToUserAsync_NoDevices_DoesNotCallSender()
    {
        using var context = CreateContext();
        var sender = new FakePushSender();
        var service = CreateService(context, sender);

        var summary = await service.SendToUserAsync(Guid.NewGuid(), new PushMessage("Title", "Body"));

        Assert.Empty(sender.Calls);
        Assert.Equal(new PushSendSummary(0, 0, 0), summary);
    }

    [Fact]
    public async Task FirebasePushSender_WithoutCredentials_FailsGracefully()
    {
        var configuration = new ConfigurationBuilder().Build();
        var sender = new FirebasePushSender(
            configuration,
            NullLogger<FirebasePushSender>.Instance);

        var results = await sender.SendAsync(new[] { "token-1" }, new PushMessage("Title", "Body"));

        var result = Assert.Single(results);
        Assert.Equal(PushSendStatus.Failed, result.Status);
        Assert.Contains("not configured", result.Error);
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
