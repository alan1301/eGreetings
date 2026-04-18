using EGreetings.Admin.Application.Features.AuditLog;
using EGreetings.Admin.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Identity;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Admin.Infrastructure.Messaging.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserRegisteredEventConsumer> _logger;

    public UserRegisteredEventConsumer(IMediator mediator, ILogger<UserRegisteredEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("Processing UserRegisteredEvent for user {UserId}", @event.UserId);

        try
        {
            var command = new CreateAuditLogCommand(
                AuditEventType.UserRegistered,
                "User",
                @event.UserId,
                $"User {@event.FullName} registered",
                @event.UserId,
                @event.Email,
                null,
                false);

            await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Audit log created for user registration: {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for user registration: {UserId}", @event.UserId);
            throw;
        }
    }
}
