using EGreetings.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Application.EventHandlers;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger) => _logger = logger;

    public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Event] User registered: {Email} (UserId={UserId})",
            notification.Email, notification.UserId);
        return Task.CompletedTask;
    }
}
