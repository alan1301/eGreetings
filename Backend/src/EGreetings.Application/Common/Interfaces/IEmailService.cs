namespace EGreetings.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody,
        string? replyTo = null, CancellationToken cancellationToken = default);

    Task SendGreetingEmailAsync(int greetingId, CancellationToken cancellationToken = default);
    Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string toEmail, string token, CancellationToken cancellationToken = default);
}
