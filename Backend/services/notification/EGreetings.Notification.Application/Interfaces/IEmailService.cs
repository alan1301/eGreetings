namespace EGreetings.Notification.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody,
        string? replyTo = null, CancellationToken cancellationToken = default);

    Task SendGreetingEmailAsync(int greetingId, string recipientEmail, string recipientName, string templateName, string senderName, string uniqueToken, string? replyToEmail = null, CancellationToken cancellationToken = default);
    Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string toEmail, string token, CancellationToken cancellationToken = default);
}
