using BankoApi.Data;
using BankoApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BankoApi.Services;

public class ScheduledTaskService : BackgroundService
{
    private readonly TimeSpan _executionTime = new(08, 00, 00);
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduledTaskService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
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
                FetchAndStoreTransactions();
            }
            catch (Exception ex)
            {
                // Handle exceptions (log errors, etc.)
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }
    }

    private async void FetchAndStoreTransactions()
    {
        using var scope = _scopeFactory.CreateScope();
        var goCardlessService = scope.ServiceProvider.GetRequiredService<GoCardlessService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankoDbContext>();

        List<Guid> userIds = await dbContext.Users.Select(a => a.UserId).ToListAsync();
        foreach (Guid userId in userIds)
        {
            // Trigger the endpoint
            var bankAccountIds = await dbContext.BankAuthorizations.Where(ba => ba.UserId == userId)
                .SelectMany(x => x.BankAccounts)
                .Select(y => y.BankAccountId)
                .ToListAsync();

            if (!bankAccountIds.IsNullOrEmpty())
            {
                foreach (var gcAccountId in bankAccountIds)
                {
                    Guid bankAccountId = Guid.Parse(gcAccountId);
                    var transactions = await goCardlessService.GetTransactionsAsync(bankAccountId);
                    if (transactions != null)
                    {
                        var repository = new TransactionsRepository();
                        await repository.StoreTransactions(ctx: dbContext, userId: userId, transactions: transactions, bankAccountId: bankAccountId);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}