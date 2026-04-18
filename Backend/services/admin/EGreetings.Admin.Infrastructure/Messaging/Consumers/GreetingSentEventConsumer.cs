using EGreetings.Admin.Application.Features.AuditLog;
using EGreetings.Admin.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Greeting;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Admin.Infrastructure.Messaging.Consumers;

public class GreetingSentEventConsumer : IConsumer<GreetingSentEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GreetingSentEventConsumer> _logger;

    public GreetingSentEventConsumer(IMediator mediator, ILogger<GreetingSentEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GreetingSentEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("Processing GreetingSentEvent for greeting {GreetingId}", @event.GreetingId);

        try
        {
            var command = new CreateAuditLogCommand(
                AuditEventType.GreetingSent,
                "Greeting",
                @event.GreetingId,
                $"Greeting sent to {context.Message.RecipientName}",
                null,
                null,
                null,
                true);

            await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Audit log created for greeting sent: {GreetingId}", @event.GreetingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for greeting sent: {GreetingId}", @event.GreetingId);
            throw;
        }
    }
}
