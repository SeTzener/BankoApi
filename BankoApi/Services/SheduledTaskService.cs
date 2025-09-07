using BankoApi.Data;
using BankoApi.Repository;
using DotNetEnv;

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

            // Wait until the next scheduled time
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
        Env.Load();
        // TODO(): Change this to retrieve a list of accountIDs from the database
        Console.WriteLine(DateTime.UtcNow);
        var gcAccountId = Environment.GetEnvironmentVariable("GOCARDLESS_ACCOUNT_ID") ??
                        throw new Exception("Environment variable GOCARDLESS_ACCOUNT_ID not set");

        using var scope = _scopeFactory.CreateScope();
        var goCardlessService = scope.ServiceProvider.GetRequiredService<GoCardlessService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankoDbContext>();
        List<Guid> userIds = dbContext.Users.Select(a => a.UserId).ToList();

        foreach (Guid userId in userIds)
        {
            // Trigger the endpoint
            var transactions = await goCardlessService.GetTransactionsAsync(gcAccountId);
            if (transactions != null)
            {
                // Here I should loop through the GoCardless IDs to retrieve all the bank accounts transactions
                var repository = new TransactionsRepository();
                repository.StoreTransactions(ctx:dbContext, userId: userId, transactions: transactions);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}