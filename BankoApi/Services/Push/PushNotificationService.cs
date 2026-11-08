using BankoApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Services.Push;

public sealed record PushSendSummary(int Sent, int Failed, int Deactivated);

/// <summary>
/// Resolves the active devices of a user and forwards a push message to them.
/// Tokens that FCM reports as invalid are deactivated so they are not retried.
/// </summary>
public class PushNotificationService
{
    private readonly BankoDbContext _dbContext;
    private readonly IPushSender _pushSender;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        BankoDbContext dbContext,
        IPushSender pushSender,
        ILogger<PushNotificationService> logger)
    {
        _dbContext = dbContext;
        _pushSender = pushSender;
        _logger = logger;
    }

    public async Task<PushSendSummary> SendToUserAsync(
        Guid userId,
        PushMessage message,
        CancellationToken cancellationToken = default)
    {
        var devices = await _dbContext.PushDevices
            .Where(device => device.UserId == userId && device.IsActive)
            .ToListAsync(cancellationToken);

        if (devices.Count == 0)
        {
            return new PushSendSummary(0, 0, 0);
        }

        var tokens = devices
            .Select(device => device.Token)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var results = await _pushSender.SendAsync(tokens, message, cancellationToken);

        var invalidTokens = results
            .Where(result => result.Status == PushSendStatus.InvalidToken)
            .Select(result => result.Token)
            .ToHashSet(StringComparer.Ordinal);

        if (invalidTokens.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var device in devices.Where(device => invalidTokens.Contains(device.Token)))
            {
                device.IsActive = false;
                device.UpdatedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Deactivated {Count} push devices with invalid FCM tokens for user {UserId}",
                invalidTokens.Count, userId);
        }

        var sent = results.Count(result => result.Status == PushSendStatus.Sent);
        var failed = results.Count(result => result.Status == PushSendStatus.Failed);

        if (failed > 0)
        {
            _logger.LogWarning(
                "Failed to send {Failed} of {Total} push notifications for user {UserId}",
                failed, results.Count, userId);
        }

        return new PushSendSummary(sent, failed, invalidTokens.Count);
    }
}
