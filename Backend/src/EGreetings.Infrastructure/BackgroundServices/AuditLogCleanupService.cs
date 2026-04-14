using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.BackgroundServices;

/// <summary>
/// BR-33: Tự động xóa AuditLog cũ hơn 30 ngày
/// </summary>
public class AuditLogCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    public AuditLogCleanupService(IServiceScopeFactory scopeFactory,
        ILogger<AuditLogCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupOldLogsAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CleanupOldLogsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<EGreetings.Infrastructure.Persistence.AppDbContext>();

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var oldLogs = await context.AuditLogs
            .Where(a => a.CreatedAt < cutoff)
            .ToListAsync(ct);

        if (oldLogs.Count == 0) return;

        context.AuditLogs.RemoveRange(oldLogs);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Đã xóa {Count} audit logs cũ hơn 30 ngày.", oldLogs.Count);
    }
}
