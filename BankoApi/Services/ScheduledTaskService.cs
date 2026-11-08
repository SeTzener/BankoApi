using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Repository;
using BankoApi.Services.Push;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BankoApi.Services;

public class ScheduledTaskService : BackgroundService
{
    private readonly TimeSpan _executionTime = new(08, 00, 00);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledTaskService> _logger;

    public ScheduledTaskService(IServiceScopeFactory scopeFactory, ILogger<ScheduledTaskService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = DateTime.UtcNow.Date.Add(_executionTime);

            if (now > nextRun)
                nextRun = nextRun.AddDays(1); // Ensure next run is tomorrow if current time passed the scheduled time

            //Wait until the next scheduled time
            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken);

            try
            {
                await FetchAndStoreTransactions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and storing transactions");
            }

            try
            {
                await CleanupStaleProcessingAuthorizations();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up stale processing authorizations");
            }

            try
            {
                await SendEuaExpiryNotifications();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending EUA expiry notifications");
            }
        }
    }

    private async Task SendEuaExpiryNotifications()
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<IEuaExpiryNotificationService>();
        await notificationService.RunAsync(DateTime.UtcNow);
    }

    private async Task FetchAndStoreTransactions()
    {
        using var scope = _scopeFactory.CreateScope();
        var goCardlessService = scope.ServiceProvider.GetRequiredService<GoCardlessService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankoDbContext>();

        var repository = scope.ServiceProvider.GetRequiredService<TransactionsRepository>();
        List<Guid> userIds = await dbContext.Users.AsNoTracking().Select(a => a.UserId).ToListAsync();
        foreach (Guid userId in userIds)
        {
            using var userScope = _scopeFactory.CreateScope();
            var userDbContext = userScope.ServiceProvider.GetRequiredService<BankoDbContext>();

            var bankAccountIds = await userDbContext.BankAuthorizations.AsNoTracking().Where(ba => ba.UserId == userId)
                .SelectMany(x => x.BankAccounts)
                .Where(account => account.IsActive)
                .Select(y => y.BankAccountId)
                .ToListAsync();

            foreach (var gcAccountId in bankAccountIds)
            {
                try
                {
                    Guid bankAccountId = Guid.Parse(gcAccountId);
                    var transactions = await goCardlessService.GetTransactionsAsync(bankAccountId);
                    if (transactions != null)
                    {
                        await repository.StoreTransactions(userDbContext, userId, bankAccountId, transactions);
                        await userDbContext.SaveChangesAsync();
                    }
                }
                catch (EndUserAgreementException ex)
                {
                    _logger.LogError(ex, "EUA expired for user {UserId}, account {AccountId}", userId, gcAccountId);
                    repository.SetEuaExpirationStatus(userDbContext, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch transactions for user {UserId}, account {AccountId}", userId, gcAccountId);
                }
            }
        }
    }

    private async Task CleanupStaleProcessingAuthorizations()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankoDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<BankAuthorizationRepository>();

        var threshold = DateTime.UtcNow.AddHours(-24);
        var removed = await repository.DeleteStaleProcessingAuthorizations(dbContext, threshold);

        if (removed > 0)
        {
            _logger.LogInformation("Cleaned up {Count} stale processing authorizations", removed);
        }
    }
}