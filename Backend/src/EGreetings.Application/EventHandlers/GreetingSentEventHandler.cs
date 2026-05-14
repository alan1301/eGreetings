using EGreetings.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Application.EventHandlers;

public class GreetingSentEventHandler : INotificationHandler<GreetingSentEvent>
{
    private readonly ILogger<GreetingSentEventHandler> _logger;

    public GreetingSentEventHandler(ILogger<GreetingSentEventHandler> logger) => _logger = logger;

    public Task Handle(GreetingSentEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Event] Greeting sent: TxId={TransactionId} → {Recipient}",
            notification.TransactionId, notification.RecipientEmail);
        return Task.CompletedTask;
    }
}
