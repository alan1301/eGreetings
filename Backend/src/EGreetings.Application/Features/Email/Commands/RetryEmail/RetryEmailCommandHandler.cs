using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Email.Commands.RetryEmail;

public class RetryEmailCommandHandler : IRequestHandler<RetryEmailCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;

    private const int MaxRetryCount = 3;          // BR-32
    private const int RetryIntervalMinutes = 5;    // BR-32

    public RetryEmailCommandHandler(IApplicationDbContext context, IEmailService emailService,
        IAuditService auditService)
    {
        _context = context;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<bool> Handle(RetryEmailCommand request, CancellationToken cancellationToken)
    {
        var emailLog = await _context.EmailLogs
            .FirstOrDefaultAsync(e => e.Id == request.EmailLogId, cancellationToken)
            ?? throw new KeyNotFoundException("EmailLog không tồn tại.");

        if (emailLog.RetryCount >= MaxRetryCount)
        {
            emailLog.Status = EmailLogStatus.Failed;
            emailLog.ErrorMessage = $"Đã vượt quá số lần retry tối đa ({MaxRetryCount} lần).";
            emailLog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(AuditEventType.EmailFailed, "EmailLog",
                emailLog.Id.ToString(),
                $"Email tới {emailLog.ToEmail} thất bại sau {MaxRetryCount} lần retry.",
                isSystemAction: true, cancellationToken: cancellationToken);
            return false;
        }

        try
        {
            emailLog.Status = EmailLogStatus.Retrying;
            emailLog.RetryCount++;
            emailLog.LastRetryAt = DateTime.UtcNow;
            emailLog.NextRetryAt = DateTime.UtcNow.AddMinutes(RetryIntervalMinutes);
            await _context.SaveChangesAsync(cancellationToken);

            await _emailService.SendEmailAsync(
                emailLog.ToEmail, emailLog.ToEmail,
                emailLog.Subject, emailLog.Body,
                cancellationToken: cancellationToken);

            emailLog.Status = EmailLogStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
            emailLog.ErrorMessage = null;
            emailLog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(AuditEventType.EmailRetried, "EmailLog",
                emailLog.Id.ToString(),
                $"Email tới {emailLog.ToEmail} gửi thành công sau lần retry {emailLog.RetryCount}.",
                isSystemAction: true, cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            emailLog.Status = emailLog.RetryCount >= MaxRetryCount
                ? EmailLogStatus.Failed : EmailLogStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
            emailLog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
    }
}
