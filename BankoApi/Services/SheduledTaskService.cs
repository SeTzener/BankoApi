using BankoApi.Controllers.GoCardless;
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
        Env.Load();
        // TODO(): Change this to retrieve a list of accountIDs from the database
        var accountId = Environment.GetEnvironmentVariable("GOCARDLESS_ACCOUNT_ID") ??
                        throw new Exception("Environment variable GOCARDLESS_ACCOUNT_ID not set");
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
                using var scope = _scopeFactory.CreateScope();
                var transactionsController = scope.ServiceProvider.GetRequiredService<TransactionsController>();

                // Trigger the endpoint
                await transactionsController.FetchAndStoreTransactions(accountId);
            }
            catch (Exception ex)
            {
                // Handle exceptions (log errors, etc.)
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }
    }
}