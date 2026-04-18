using EGreetings.Notification.Application.Common.Interfaces;
using EGreetings.Notification.Application.DTOs;
using EGreetings.Shared.Contracts.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EGreetings.Notification.Infrastructure.Messaging.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ILogger<UserRegisteredEventConsumer> _logger;

    public UserRegisteredEventConsumer(
        IEmailService emailService,
        IIdempotencyRepository idempotencyRepository,
        ILogger<UserRegisteredEventConsumer> logger)
    {
        _emailService = emailService;
        _idempotencyRepository = idempotencyRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;
        var messageId = context.MessageId?.ToString();

        _logger.LogInformation("Processing UserRegisteredEvent for user {UserId} with email {Email}", @event.UserId, @event.Email);

        // Idempotency check
        if (!string.IsNullOrEmpty(messageId))
        {
            var alreadyProcessed = await _idempotencyRepository.ExistsAsync(messageId, context.CancellationToken);
            if (alreadyProcessed)
            {
                _logger.LogWarning("UserRegisteredEvent {MessageId} already processed, skipping", messageId);
                return;
            }
        }

        try
        {
            var htmlBody = EmailTemplates.WelcomeEmail(@event.FullName);
            await _emailService.SendEmailAsync(@event.Email, "Welcome to E-Greetings!", htmlBody, context.CancellationToken);

            if (!string.IsNullOrEmpty(messageId))
            {
                await _idempotencyRepository.MarkProcessedAsync(messageId, context.CancellationToken);
            }

            _logger.LogInformation("Welcome email sent to {Email}", @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", @event.Email);
            throw;
        }
    }
}
