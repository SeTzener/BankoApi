using BankoApi.Data;
using BankoApi.Data.Dao;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Services.Push;

public interface IEuaExpiryNotificationService
{
    Task RunAsync(DateTime now, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sends the daily end user agreement (EUA) notifications:
/// a warning when an agreement expires within <see cref="WarningDays"/> days and
/// an alert once it has expired. Notifications are grouped per user so a user with
/// several connections receives a single push per type. The expiry date a
/// notification was sent for is stored on the authorization, which suppresses
/// duplicates until the agreement is renewed with a new expiry date.
/// </summary>
public class EuaExpiryNotificationService : IEuaExpiryNotificationService
{
    public const int WarningDays = 7;

    private readonly BankoDbContext _dbContext;
    private readonly PushNotificationService _pushNotificationService;
    private readonly ILogger<EuaExpiryNotificationService> _logger;

    public EuaExpiryNotificationService(
        BankoDbContext dbContext,
        PushNotificationService pushNotificationService,
        ILogger<EuaExpiryNotificationService> logger)
    {
        _dbContext = dbContext;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task RunAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        await SendWarningsAsync(now, cancellationToken);
        await SendExpiredNotificationsAsync(now, cancellationToken);
    }

    private async Task SendWarningsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var threshold = now.AddDays(WarningDays);

        var candidates = await _dbContext.BankAuthorizations
            .Where(authorization =>
                authorization.Status == BankAuthorizationStaus.Linked
                && authorization.AgreementExpiresAt != null
                && authorization.AgreementExpiresAt > now
                && authorization.AgreementExpiresAt <= threshold
                && (authorization.ExpiryWarningNotifiedFor == null
                    || authorization.ExpiryWarningNotifiedFor != authorization.AgreementExpiresAt)
                && authorization.BankAccounts!.Any(account => account.IsActive))
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0) return;

        foreach (var group in candidates.GroupBy(authorization => authorization.UserId))
        {
            var daysUntilExpiry = group.Min(authorization =>
                (int)Math.Ceiling((authorization.AgreementExpiresAt!.Value - now).TotalDays));

            var message = new PushMessage(
                "Bank connection expiring soon",
                $"A bank connection expires in {daysUntilExpiry} day{(daysUntilExpiry == 1 ? "" : "s")}. " +
                "Open Banko to renew it and keep syncing transactions.",
                new Dictionary<string, string> { ["type"] = "eua_expiry_warning" });

            var summary = await _pushNotificationService.SendToUserAsync(group.Key, message, cancellationToken);

            // Only mark the agreement when the push actually reached a device. A user
            // without registered devices must be notified once they register one.
            if (summary.Sent == 0)
            {
                _logger.LogInformation(
                    "No push device reached for the EUA expiry warning of user {UserId}", group.Key);
                continue;
            }

            foreach (var authorization in group)
            {
                authorization.ExpiryWarningNotifiedFor = authorization.AgreementExpiresAt;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Sent an EUA expiry warning to user {UserId} for {Count} connection(s)",
                group.Key, group.Count());
        }
    }

    private async Task SendExpiredNotificationsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var candidates = await _dbContext.BankAuthorizations
            .Where(authorization =>
                (authorization.Status == BankAuthorizationStaus.Linked
                 || authorization.Status == BankAuthorizationStaus.Expired)
                && authorization.AgreementExpiresAt != null
                && authorization.AgreementExpiresAt <= now
                && (authorization.ExpiryExpiredNotifiedFor == null
                    || authorization.ExpiryExpiredNotifiedFor != authorization.AgreementExpiresAt)
                && authorization.BankAccounts!.Any(account => account.IsActive))
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0) return;

        foreach (var group in candidates.GroupBy(authorization => authorization.UserId))
        {
            var message = new PushMessage(
                "Bank connection expired",
                "A bank connection has expired. Open Banko to renew it and keep syncing transactions.",
                new Dictionary<string, string> { ["type"] = "eua_expiry_expired" });

            var summary = await _pushNotificationService.SendToUserAsync(group.Key, message, cancellationToken);

            if (summary.Sent == 0)
            {
                _logger.LogInformation(
                    "No push device reached for the expired EUA notification of user {UserId}", group.Key);
                continue;
            }

            foreach (var authorization in group)
            {
                authorization.ExpiryExpiredNotifiedFor = authorization.AgreementExpiresAt;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Sent an expired EUA notification to user {UserId} for {Count} connection(s)",
                group.Key, group.Count());
        }
    }
}
