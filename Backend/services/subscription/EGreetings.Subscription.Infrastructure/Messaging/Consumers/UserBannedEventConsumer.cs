using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EGreetings.Subscription.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Identity;

namespace EGreetings.Subscription.Infrastructure.Messaging.Consumers;

public class UserBannedEventConsumer : IConsumer<UserBannedEvent>
{
    private readonly SubscriptionDbContext _context;
    private readonly ILogger<UserBannedEventConsumer> _logger;

    public UserBannedEventConsumer(SubscriptionDbContext context, ILogger<UserBannedEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserBannedEvent> context)
    {
        try
        {
            var activeSubscriptions = await _context.Subscriptions
                .Where(s => s.UserId == context.Message.UserId && s.Status == SubscriptionStatus.Active)
                .ToListAsync();

            if (activeSubscriptions.Count > 0)
            {
                foreach (var subscription in activeSubscriptions)
                {
                    subscription.IsDisabledByAdmin = true;
                    subscription.Status = SubscriptionStatus.Disabled;
                    subscription.DisabledReason = context.Message.Reason ?? "User was banned";
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Disabled {Count} active subscriptions for banned user {UserId}",
                    activeSubscriptions.Count, context.Message.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserBannedEvent for user {UserId}", context.Message.UserId);
            throw;
        }
    }
}
