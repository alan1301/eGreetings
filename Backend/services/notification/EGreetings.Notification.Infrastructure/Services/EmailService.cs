using EGreetings.Notification.Application.Interfaces;
using EGreetings.Notification.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EGreetings.Notification.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody,
        string? replyTo = null, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:Username"] ?? "";
        var smtpPass = _configuration["Email:Password"] ?? "";
        var fromName = _configuration["Email:FromName"] ?? "E-Greetings Vietnam";
        var fromAddress = _configuration["Email:FromAddress"] ?? smtpUser;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));

        // BR-28: Reply-To header
        if (!string.IsNullOrEmpty(replyTo))
            message.ReplyTo.Add(new MailboxAddress("", replyTo));

        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email gửi thành công tới {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gửi email tới {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
    }

    public async Task SendGreetingEmailAsync(int greetingId, string recipientEmail, string recipientName, string templateName, string senderName, string uniqueToken, string? replyToEmail = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://egreetings.vn";
        var viewUrl = $"{baseUrl}/card/{uniqueToken}";

        var htmlBody = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #2980b9;">💌 Bạn có một tấm thiệp từ {senderName}!</h2>
                <p>Xin chào <strong>{recipientName}</strong>,</p>
                <p>{senderName} vừa gửi cho bạn một tấm thiệp điện tử đặc biệt.</p>
                <div style="text-align: center; margin: 2rem 0;">
                    <a href="{viewUrl}" style="background: #2980b9; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-size: 16px;">
                        Xem tấm thiệp của bạn 🎁
                    </a>
                </div>
                <p style="color: #666;">Thiệp được tạo từ mẫu: {templateName}</p>
                <hr />
                <p style="font-size: 12px; color: #999;">E-Greetings Vietnam - Gửi yêu thương qua từng tấm thiệp</p>
            </div>
            """;

        try
        {
            await SendEmailAsync(
                recipientEmail, recipientName,
                $"💌 {senderName} gửi bạn một tấm thiệp đặc biệt!",
                htmlBody, replyToEmail, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xử lý gửi thẻ cho id {id}", greetingId);
        }
    }

    public async Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://egreetings.vn";
        var verifyUrl = $"{baseUrl}/verify-email?token={token}";

        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2>Xác thực địa chỉ email</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản E-Greetings!</p>
                <p>Nhấn vào nút bên dưới để xác thực địa chỉ email của bạn:</p>
                <div style="text-align: center; margin: 2rem 0;">
                    <a href="{verifyUrl}" style="background: #27ae60; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none;">
                        Xác thực email
                    </a>
                </div>
                <p style="color: #666;">Link có hiệu lực trong 24 giờ.</p>
            </div>
            """;

        try
        {
            await SendEmailAsync(toEmail, toEmail, "Xác thực địa chỉ email - E-Greetings", html,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Đã bỏ qua lỗi gửi email xác thực trong môi trường Development.");
        }
    }

    public async Task SendPasswordResetAsync(string toEmail, string token, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://egreetings.vn";
        var resetUrl = $"{baseUrl}/reset-password?token={token}";

        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2>Đặt lại mật khẩu</h2>
                <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                <div style="text-align: center; margin: 2rem 0;">
                    <a href="{resetUrl}" style="background: #e74c3c; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none;">
                        Đặt lại mật khẩu
                    </a>
                </div>
                <p style="color: #666;">Link có hiệu lực trong 15 phút. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
            </div>
            """;

        await SendEmailAsync(toEmail, toEmail, "Đặt lại mật khẩu - E-Greetings", html,
            cancellationToken: cancellationToken);
    }
}
