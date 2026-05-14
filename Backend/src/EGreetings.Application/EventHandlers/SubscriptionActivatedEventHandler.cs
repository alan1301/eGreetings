using EGreetings.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Application.EventHandlers;

public class SubscriptionActivatedEventHandler : INotificationHandler<SubscriptionActivatedEvent>
{
    private readonly ILogger<SubscriptionActivatedEventHandler> _logger;

    public SubscriptionActivatedEventHandler(ILogger<SubscriptionActivatedEventHandler> logger) => _logger = logger;

    public Task Handle(SubscriptionActivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Event] Subscription activated: SubId={SubscriptionId} UserId={UserId} Plan={Plan}",
            notification.SubscriptionId, notification.UserId, notification.Plan);
        return Task.CompletedTask;
    }
}
