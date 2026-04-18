using EGreetings.Admin.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EGreetings.Admin.Infrastructure.Services;

public class AuditLogCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);
    private readonly int _retentionDays = 30;

    public AuditLogCleanupService(IServiceProvider serviceProvider, ILogger<AuditLogCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditLogCleanupService started");

        // Wait before first execution
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldAuditLogsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AuditLogCleanupService");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("AuditLogCleanupService stopped");
    }

    private async Task CleanupOldAuditLogsAsync(CancellationToken ct)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IAdminDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

            var deletedCount = await dbContext.AuditLogs
                .Where(a => a.OccurredAt < cutoffDate)
                .ExecuteDeleteAsync(ct);

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Deleted {Count} audit logs older than {Days} days",
                    deletedCount, _retentionDays);
            }
        }
    }
}
