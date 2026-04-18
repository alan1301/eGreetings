using EGreetings.Notification.Application.Common.Interfaces;
using EGreetings.Notification.Application.DTOs;
using EGreetings.Shared.Contracts.Events.Greeting;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EGreetings.Notification.Infrastructure.Messaging.Consumers;

public class GreetingSentEventConsumer : IConsumer<GreetingSentEvent>
{
    private readonly IEmailService _emailService;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ILogger<GreetingSentEventConsumer> _logger;

    public GreetingSentEventConsumer(
        IEmailService emailService,
        IIdempotencyRepository idempotencyRepository,
        ILogger<GreetingSentEventConsumer> logger)
    {
        _emailService = emailService;
        _idempotencyRepository = idempotencyRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GreetingSentEvent> context)
    {
        var @event = context.Message;
        var messageId = context.MessageId?.ToString();

        _logger.LogInformation("Processing GreetingSentEvent for greeting {GreetingId} to {RecipientEmail}", @event.GreetingId, @event.RecipientEmail);

        // Idempotency check
        if (!string.IsNullOrEmpty(messageId))
        {
            var alreadyProcessed = await _idempotencyRepository.ExistsAsync(messageId, context.CancellationToken);
            if (alreadyProcessed)
            {
                _logger.LogWarning("GreetingSentEvent {MessageId} already processed, skipping", messageId);
                return;
            }
        }

        try
        {
            var viewUrl = @event.ViewUrl ?? "https://egreetings.vn/greetings";
            var htmlBody = EmailTemplates.GreetingNotification(@event.RecipientName, @event.SenderMessage, viewUrl);
            var subject = $"{@event.SenderName} sent you a greeting!";

            await _emailService.SendEmailAsync(@event.RecipientEmail, subject, htmlBody, context.CancellationToken);

            if (!string.IsNullOrEmpty(messageId))
            {
                await _idempotencyRepository.MarkProcessedAsync(messageId, context.CancellationToken);
            }

            _logger.LogInformation("Greeting notification sent to {Email}", @event.RecipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send greeting notification to {Email}", @event.RecipientEmail);
            throw;
        }
    }
}
