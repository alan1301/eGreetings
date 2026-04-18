using EGreetings.Admin.Application.Features.AuditLog;
using EGreetings.Admin.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Subscription;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Admin.Infrastructure.Messaging.Consumers;

public class PaymentConfirmedEventConsumer : IConsumer<PaymentConfirmedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentConfirmedEventConsumer> _logger;

    public PaymentConfirmedEventConsumer(IMediator mediator, ILogger<PaymentConfirmedEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentConfirmedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("Processing PaymentConfirmedEvent for user {UserId}", @event.UserId);

        try
        {
            var command = new CreateAuditLogCommand(
                AuditEventType.PaymentConfirmed,
                "Payment",
                @event.SubscriptionId,
                $"Payment of {context.Message.Amount} confirmed for user ID {context.Message.UserId}",
                @event.UserId,
                null,
                null,
                false);

            await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Audit log created for payment confirmed: {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for payment confirmed: {UserId}", @event.UserId);
            throw;
        }
    }
}
