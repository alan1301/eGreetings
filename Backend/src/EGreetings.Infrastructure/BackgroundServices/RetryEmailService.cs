using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.BackgroundServices;

/// <summary>
/// UC29 - Tự động retry email thất bại
/// BR-32: Tối đa 3 lần retry, cách nhau 5 phút
/// </summary>
public class RetryEmailService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RetryEmailService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    public RetryEmailService(IServiceScopeFactory scopeFactory,
        ILogger<RetryEmailService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetryEmailService đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessRetryEmailsAsync(stoppingToken);
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessRetryEmailsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<EGreetings.Infrastructure.Persistence.AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;

        // Lấy email thất bại cần retry (chưa vượt 3 lần, đến giờ retry)
        var toRetry = await context.EmailLogs
            .Where(e => e.Status == EmailLogStatus.Failed
                && e.RetryCount < 3
                && (e.NextRetryAt == null || e.NextRetryAt <= now))
            .Take(50)  // Xử lý tối đa 50 email mỗi lần
            .ToListAsync(ct);

        if (toRetry.Count == 0) return;

        _logger.LogInformation("Đang retry {Count} emails.", toRetry.Count);

        foreach (var emailLog in toRetry)
        {
            try
            {
                emailLog.Status = EmailLogStatus.Retrying;
                emailLog.RetryCount++;
                emailLog.LastRetryAt = now;
                emailLog.NextRetryAt = now.AddMinutes(5);
                await context.SaveChangesAsync(ct);

                await emailService.SendEmailAsync(
                    emailLog.ToEmail, emailLog.ToEmail,
                    emailLog.Subject, emailLog.Body,
                    cancellationToken: ct);

                emailLog.Status = EmailLogStatus.Sent;
                emailLog.SentAt = now;
                emailLog.ErrorMessage = null;

                _logger.LogInformation("Retry email tới {Email} thành công (lần {N}).",
                    emailLog.ToEmail, emailLog.RetryCount);
            }
            catch (Exception ex)
            {
                emailLog.Status = emailLog.RetryCount >= 3
                    ? EmailLogStatus.Failed : EmailLogStatus.Failed;
                emailLog.ErrorMessage = ex.Message;

                _logger.LogWarning("Retry email tới {Email} thất bại (lần {N}): {Msg}",
                    emailLog.ToEmail, emailLog.RetryCount, ex.Message);
            }

            emailLog.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }
}
