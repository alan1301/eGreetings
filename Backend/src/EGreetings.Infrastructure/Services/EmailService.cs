using EGreetings.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EGreetings.Infrastructure.Services;

/// <summary>
/// Email service using MailKit.
/// BR-28: Always sets From = noreply@e-greetings.com and Reply-To = sender's email.
/// Supports both TLS (production) and plain-text (dev MailHog) SMTP via config.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var smtpSection = _config.GetSection("SmtpSettings");

        var host = smtpSection["Host"] ?? "localhost";
        var port = int.Parse(smtpSection["Port"] ?? "25");
        var username = smtpSection["Username"] ?? string.Empty;
        var password = smtpSection["Password"] ?? string.Empty;
        var useTls = bool.Parse(smtpSection["UseTls"] ?? "true");
        var skipAuthWhenEmpty = bool.Parse(smtpSection["SkipAuthWhenEmpty"] ?? "false");

        var mime = new MimeMessage();

        // BR-28: From is always system address
        mime.From.Add(new MailboxAddress(message.FromName, message.From));
        mime.To.Add(new MailboxAddress(message.ToName, message.To));
        mime.Subject = message.Subject;

        // BR-28: Reply-To = original sender's email
        if (!string.IsNullOrEmpty(message.ReplyTo))
            mime.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));

        mime.Body = new TextPart("html") { Text = message.HtmlBody };

        using var smtp = new SmtpClient();

        // Choose TLS mode: production uses StartTls, dev MailHog uses None
        var secureOption = useTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await smtp.ConnectAsync(host, port, secureOption, cancellationToken);

        // Skip SMTP AUTH for local dev servers that don't require credentials (e.g. MailHog)
        var shouldAuthenticate = !skipAuthWhenEmpty
            || (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password));

        if (shouldAuthenticate)
            await smtp.AuthenticateAsync(username, password, cancellationToken);

        await smtp.SendAsync(mime, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("[EMAIL] Sent '{Subject}' to {To}", message.Subject, message.To);
    }
}
