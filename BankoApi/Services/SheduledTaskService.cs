using BankoApi.Controllers.GoCardless;

namespace BankoApi.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class ScheduledTaskService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _executionTime = new TimeSpan(13, 35, 00);

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
            {
                nextRun = nextRun.AddDays(1); // Ensure next run is tomorrow if current time passed the scheduled time
            }

            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken); // Wait until the next scheduled time

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var institutionsController = scope.ServiceProvider.GetRequiredService<InstitutionsController>();

                
                await institutionsController.FetchInstitutionsAsync(); // Trigger the endpoint
            }
            catch (Exception ex)
            {
                // Handle exceptions (log errors, etc.)
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }
    }
}
