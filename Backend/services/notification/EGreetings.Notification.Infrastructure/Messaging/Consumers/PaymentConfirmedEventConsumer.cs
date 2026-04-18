using EGreetings.Notification.Application.Common.Interfaces;
using EGreetings.Notification.Application.DTOs;
using EGreetings.Shared.Contracts.Events.Subscription;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EGreetings.Notification.Infrastructure.Messaging.Consumers;

public class PaymentConfirmedEventConsumer : IConsumer<PaymentConfirmedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ILogger<PaymentConfirmedEventConsumer> _logger;

    public PaymentConfirmedEventConsumer(
        IEmailService emailService,
        IIdempotencyRepository idempotencyRepository,
        ILogger<PaymentConfirmedEventConsumer> logger)
    {
        _emailService = emailService;
        _idempotencyRepository = idempotencyRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentConfirmedEvent> context)
    {
        var @event = context.Message;
        var messageId = context.MessageId?.ToString();

        _logger.LogInformation("Processing PaymentConfirmedEvent for user {UserId} with amount {Amount}", @event.UserId, @event.Amount);

        // Idempotency check
        if (!string.IsNullOrEmpty(messageId))
        {
            var alreadyProcessed = await _idempotencyRepository.ExistsAsync(messageId, context.CancellationToken);
            if (alreadyProcessed)
            {
                _logger.LogWarning("PaymentConfirmedEvent {MessageId} already processed, skipping", messageId);
                return;
            }
        }

        try
        {
            var htmlBody = EmailTemplates.PaymentConfirmed(@event.FullName, @event.Amount, @event.SubscriptionExpiredAt);
            await _emailService.SendEmailAsync(@event.Email, "Payment Confirmed - Subscription Active", htmlBody, context.CancellationToken);

            if (!string.IsNullOrEmpty(messageId))
            {
                await _idempotencyRepository.MarkProcessedAsync(messageId, context.CancellationToken);
            }

            _logger.LogInformation("Payment confirmation email sent to {Email}", @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment confirmation email to {Email}", @event.Email);
            throw;
        }
    }
}
