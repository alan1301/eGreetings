using EGreetings.Notification.Application.Common.Interfaces;
using EGreetings.Notification.Domain.Entities;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EGreetings.Notification.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, INotificationDbContext dbContext, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            var smtpHost = _configuration["Email:Host"];
            var smtpPort = int.Parse(_configuration["Email:Port"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var senderName = _configuration["Email:SenderName"] ?? "E-Greetings";
            var senderEmail = _configuration["Email:SenderEmail"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls, ct);
                await client.AuthenticateAsync(smtpUsername, smtpPassword, ct);
                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);
            }

            // Log successful send
            var emailLog = new EmailLog
            {
                ToEmail = to,
                Subject = subject,
                Body = htmlBody,
                Status = EmailStatus.Sent,
                AttemptCount = 1,
                LastAttemptAt = DateTime.UtcNow,
                MessageId = message.MessageId
            };

            _dbContext.EmailLogs.Add(emailLog);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);

            // Log failed send
            var emailLog = new EmailLog
            {
                ToEmail = to,
                Subject = subject,
                Body = htmlBody,
                Status = EmailStatus.Failed,
                AttemptCount = 1,
                LastAttemptAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };

            _dbContext.EmailLogs.Add(emailLog);
            await _dbContext.SaveChangesAsync(ct);

            throw;
        }
    }
}
