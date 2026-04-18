using EGreetings.Identity.Application.Features.Auth.Commands.ForgotPassword;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EGreetings.Identity.Infrastructure.Messaging.Consumers;

public class SendPasswordResetEmailCommandConsumer : IConsumer<SendPasswordResetEmailCommand>
{
    private readonly ILogger<SendPasswordResetEmailCommandConsumer> _logger;

    public SendPasswordResetEmailCommandConsumer(ILogger<SendPasswordResetEmailCommandConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SendPasswordResetEmailCommand> context)
    {
        var command = context.Message;
        _logger.LogInformation(
            "Processing password reset email for UserId: {UserId}, Email: {Email}, CorrelationId: {CorrelationId}",
            command.UserId,
            command.Email,
            command.CorrelationId
        );

        _logger.LogInformation(
            "Password reset link would be sent to {Email} with token: {Token}",
            command.Email,
            command.ResetToken
        );

        return Task.CompletedTask;
    }
}
