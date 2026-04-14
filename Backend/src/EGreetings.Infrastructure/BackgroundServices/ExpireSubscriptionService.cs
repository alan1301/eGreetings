using EGreetings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.BackgroundServices;

/// <summary>
/// UC15 - Tự động vô hiệu hóa Subscription hết hạn
/// Chạy mỗi giờ, kiểm tra và chuyển Active → Expired
/// </summary>
public class ExpireSubscriptionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpireSubscriptionService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public ExpireSubscriptionService(IServiceScopeFactory scopeFactory,
        ILogger<ExpireSubscriptionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpireSubscriptionService đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessExpiredSubscriptionsAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessExpiredSubscriptionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<EGreetings.Infrastructure.Persistence.AppDbContext>();

        var now = DateTime.UtcNow;
        var expiredSubs = await context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active && s.ExpiredAt <= now)
            .ToListAsync(ct);

        if (expiredSubs.Count == 0) return;

        foreach (var sub in expiredSubs)
        {
            sub.Status = SubscriptionStatus.Expired;
            sub.UpdatedAt = now;

            context.AuditLogs.Add(new Domain.Entities.AuditLog
            {
                UserId = null,
                EventType = AuditEventType.SubscriptionExpired,
                EntityName = "Subscription",
                EntityId = sub.Id.ToString(),
                Description = $"Subscription {sub.Id} của User {sub.UserId} đã hết hạn.",
                IsSystemAction = true
            });
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Đã expire {Count} subscriptions.", expiredSubs.Count);
    }
}
