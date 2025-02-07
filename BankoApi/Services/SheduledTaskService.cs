using System.Text.Json;
using BankoApi.Controllers.GoCardless;
using BankoApi.Data;
using BankoApi.Repository;
using BankoApi.Services.Model;
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
        var accountId = Environment.GetEnvironmentVariable("GOCARDLESS_ACCOUNT_ID") ??
                        throw new Exception("Environment variable GOCARDLESS_ACCOUNT_ID not set");
        
        using var scope = _scopeFactory.CreateScope();
        var goCardlessService = scope.ServiceProvider.GetRequiredService<GoCardlessService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankoDbContext>();


        // Trigger the endpoint
        var transactions = await goCardlessService.GetTransactionsAsync(accountId);
        if (transactions != null)
        {
            var repository = new TransactionsRepository();
            repository.StoreTransactions(dbContext, transactions);
            await dbContext.SaveChangesAsync();
        }
    }
}