using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.BackgroundServices;

/// <summary>
/// UC20 - Gửi thiệp tự động hàng ngày (sinh nhật, kỷ niệm)
/// Chạy lúc 07:00 UTC mỗi ngày, gửi thiệp cho SubscriptionRecipients có ngày sinh hôm nay
/// </summary>
public class AutoSendGreetingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoSendGreetingService> _logger;

    public AutoSendGreetingService(IServiceScopeFactory scopeFactory,
        ILogger<AutoSendGreetingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoSendGreetingService đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            // Tính thời gian đến 07:00 UTC ngày mai
            var next7am = now.Date.AddHours(7);
            if (next7am <= now) next7am = next7am.AddDays(1);

            var delay = next7am - now;
            _logger.LogInformation("AutoSend sẽ chạy lúc {Time} ({Delay} nữa)", next7am, delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await ProcessAutoSendAsync(stoppingToken);
        }
    }

    private async Task ProcessAutoSendAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<EGreetings.Infrastructure.Persistence.AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var today = DateTime.UtcNow.Date;

        // Lấy recipients có sinh nhật hôm nay và subscription đang Active
        var birthdayRecipients = await context.SubscriptionRecipients
            .Include(r => r.Subscription)
                .ThenInclude(s => s.User)
            .Where(r => r.IsActive
                && r.Birthday.HasValue
                && r.Birthday.Value.Month == today.Month
                && r.Birthday.Value.Day == today.Day
                && r.Subscription.Status == SubscriptionStatus.Active
                && r.Subscription.ExpiredAt > today)
            .ToListAsync(ct);

        _logger.LogInformation("Tìm thấy {Count} recipients có sinh nhật hôm nay.", birthdayRecipients.Count);

        // Lấy template sinh nhật mặc định (free)
        var defaultTemplate = await context.GreetingTemplates
            .FirstOrDefaultAsync(t => t.IsActive && t.IsFree
                && t.Category.Name == "Sinh nhật", ct);

        if (defaultTemplate == null)
        {
            _logger.LogWarning("Không tìm thấy template sinh nhật mặc định.");
            return;
        }

        foreach (var recipient in birthdayRecipients)
        {
            try
            {
                var greeting = new Greeting
                {
                    UserId = recipient.Subscription.UserId,
                    TemplateId = defaultTemplate.Id,
                    RecipientEmail = recipient.Email,
                    RecipientName = recipient.Name,
                    SenderMessage = $"Chúc mừng sinh nhật {recipient.Name}! Chúc bạn một ngày sinh nhật thật vui vẻ và hạnh phúc! 🎂",
                    CustomHtml = defaultTemplate.HtmlContent,
                    ReplyToEmail = recipient.Subscription.User.Email,
                    Status = GreetingStatus.Sent,
                    SentAt = DateTime.UtcNow
                };

                context.Greetings.Add(greeting);
                await context.SaveChangesAsync(ct);

                await emailService.SendGreetingEmailAsync(greeting.Id, ct);

                context.AuditLogs.Add(new AuditLog
                {
                    UserId = recipient.Subscription.UserId,
                    EventType = AuditEventType.AutoSendTriggered,
                    EntityName = "Greeting",
                    EntityId = greeting.Id.ToString(),
                    Description = $"Thiệp sinh nhật tự động gửi tới {recipient.Email}",
                    IsSystemAction = true
                });

                await context.SaveChangesAsync(ct);

                _logger.LogInformation("Đã gửi thiệp sinh nhật tự động cho {Email}", recipient.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi thiệp tự động cho {Email}: {Msg}", recipient.Email, ex.Message);
            }
        }
    }
}
