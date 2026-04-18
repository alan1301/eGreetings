using EGreetings.Notification.Application.Common.Interfaces;
using EGreetings.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EGreetings.Notification.Infrastructure.BackgroundServices;

public class RetryEmailService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RetryEmailService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public RetryEmailService(IServiceProvider serviceProvider, ILogger<RetryEmailService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetryEmailService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RetryFailedEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RetryEmailService");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("RetryEmailService stopped");
    }

    private async Task RetryFailedEmailsAsync(CancellationToken ct)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<INotificationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var failedEmails = await dbContext.EmailLogs
                .Where(e => e.Status == EmailStatus.Failed && e.AttemptCount < 3)
                .ToListAsync(ct);

            _logger.LogInformation("Found {Count} failed emails to retry", failedEmails.Count);

            foreach (var email in failedEmails)
            {
                try
                {
                    await emailService.SendEmailAsync(email.ToEmail, email.Subject, email.Body, ct);
                    email.Status = EmailStatus.Sent;
                    email.AttemptCount++;
                    email.LastAttemptAt = DateTime.UtcNow;
                    email.ErrorMessage = null;

                    _logger.LogInformation("Retry successful for email to {To}", email.ToEmail);
                }
                catch (Exception ex)
                {
                    email.AttemptCount++;
                    email.LastAttemptAt = DateTime.UtcNow;
                    email.ErrorMessage = ex.Message;

                    if (email.AttemptCount >= 3)
                    {
                        email.Status = EmailStatus.MaxRetriesExceeded;
                        _logger.LogWarning("Email to {To} exceeded max retries", email.ToEmail);
                    }
                    else
                    {
                        email.Status = EmailStatus.Failed;
                        _logger.LogWarning(ex, "Retry failed for email to {To}, attempt {Attempt}/3", email.ToEmail, email.AttemptCount);
                    }
                }
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }
}
