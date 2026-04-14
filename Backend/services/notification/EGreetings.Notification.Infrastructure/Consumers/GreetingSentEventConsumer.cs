using EGreetings.Notification.Application.Interfaces;
using EGreetings.Shared.Contracts.Events.Greeting;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EGreetings.Notification.Infrastructure.Consumers;

public class GreetingSentEventConsumer : IConsumer<GreetingSentEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<GreetingSentEventConsumer> _logger;

    public GreetingSentEventConsumer(IEmailService emailService, ILogger<GreetingSentEventConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GreetingSentEvent> context)
    {
        _logger.LogInformation("Nhận được event gửi thiệp {GreetingId} tới {Email}", context.Message.GreetingId, context.Message.RecipientEmail);

        // Trong thực tế sẽ tra DB, nhưng để mock thì chúng ta pass thẳng param qua Event
        var senderName = context.Message.UserId.HasValue ? $"User {context.Message.UserId}" : "Khách";
        var uniqueToken = Guid.NewGuid().ToString("N");

        await _emailService.SendGreetingEmailAsync(
            context.Message.GreetingId,
            context.Message.RecipientEmail,
            "Người nhận",
            context.Message.TemplateName,
            senderName,
            uniqueToken,
            null,
            context.CancellationToken);
            
        _logger.LogInformation("Đã xử lý xong event gửi thiệp {GreetingId}", context.Message.GreetingId);
    }
}
