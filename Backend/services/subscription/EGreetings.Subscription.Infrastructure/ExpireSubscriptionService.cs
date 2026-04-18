using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Infrastructure;

public class ExpireSubscriptionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpireSubscriptionService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(1);

    public ExpireSubscriptionService(IServiceProvider serviceProvider, ILogger<ExpireSubscriptionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExpireSubscriptionsAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ExpireSubscriptionService is stopping");
        }
        finally
        {
            timer.Dispose();
        }
    }

    private async Task ExpireSubscriptionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();

        try
        {
            var expiredSubscriptions = await context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active && s.ExpiredAt <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredSubscriptions.Count > 0)
            {
                foreach (var subscription in expiredSubscriptions)
                {
                    subscription.Status = SubscriptionStatus.Expired;
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Expired {Count} subscriptions", expiredSubscriptions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring subscriptions");
        }
    }
}
