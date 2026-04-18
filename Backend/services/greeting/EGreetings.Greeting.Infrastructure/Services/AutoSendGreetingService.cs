using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Greeting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Infrastructure.Services;

public class AutoSendGreetingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoSendGreetingService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public AutoSendGreetingService(IServiceProvider serviceProvider, ILogger<AutoSendGreetingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoSendGreetingService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledGreetings(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled greetings");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("AutoSendGreetingService stopped");
    }

    private async Task ProcessScheduledGreetings(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGreetingDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var utcNow = DateTime.UtcNow;

        // BR-13: Find scheduled greetings that should be sent now
        var greetingsToSend = await context.Greetings
            .Where(g => g.Status == GreetingStatus.Scheduled && g.ScheduledAt <= utcNow)
            .ToListAsync(cancellationToken);

        if (greetingsToSend.Count == 0)
        {
            return;
        }

        _logger.LogInformation($"Processing {greetingsToSend.Count} scheduled greetings");

        foreach (var greeting in greetingsToSend)
        {
            greeting.Status = GreetingStatus.Pending;
            greeting.SentAt = utcNow;
            greeting.UpdatedAt = utcNow;

            // Publish event for each greeting
            var viewUrl = $"https://egreetings.vn/view/{greeting.ViewToken}";
            var @event = new GreetingSentEvent(
                greeting.Id,
                greeting.RecipientEmail,
                greeting.RecipientName,
                "E-Greetings",
                greeting.SenderMessage,
                viewUrl,
                utcNow
            );

            await publishEndpoint.Publish(@event, cancellationToken);

            _logger.LogInformation($"Published GreetingSentEvent for greeting {greeting.Id}");
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Processed {greetingsToSend.Count} greetings successfully");
    }
}
