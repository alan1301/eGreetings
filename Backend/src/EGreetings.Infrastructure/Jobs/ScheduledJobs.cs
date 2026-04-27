using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Infrastructure.Persistence;
using EGreetings.Shared.Constants;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.Jobs;

/// <summary>
/// UC15 – Auto-disable expired subscriptions.
/// Runs at 00:01 daily (BR-16). Sends email to user.
/// </summary>
public class AutoDisableExpiredSubscriptionsJob
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;
    private readonly ILogger<AutoDisableExpiredSubscriptionsJob> _logger;

    public AutoDisableExpiredSubscriptionsJob(
        AppDbContext db, IEmailService emailService,
        IAuditLogService audit, ILogger<AutoDisableExpiredSubscriptionsJob> logger)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
        _logger = logger;
    }

    [DisableConcurrentExecution(30 * 60)]
    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation("[UC15] AutoDisableExpiredSubscriptions started at {Time}", now);

        var expired = await _db.Subscriptions
            .Include(s => s.User)
            .Where(s => s.Status == SubscriptionStatus.Active && s.ExpiryDate <= now)
            .ToListAsync();

        int processed = 0, errors = 0;

        foreach (var sub in expired)
        {
            try
            {
                sub.Status = SubscriptionStatus.Expired;
                sub.UpdatedAt = now;

                await _emailService.SendAsync(new EmailMessage
                {
                    To = sub.User.Email,
                    ToName = sub.User.FullName,
                    Subject = "Dịch vụ Subscribe đã hết hạn",
                    HtmlBody = $"""
                        <h2>Dịch vụ E-Greetings Subscribe của bạn đã hết hạn</h2>
                        <p>Ngày hết hạn: {sub.ExpiryDate:dd/MM/yyyy}</p>
                        <p>Vui lòng gia hạn dịch vụ để tiếp tục nhận thiệp tự động hàng ngày.</p>
                        <a href="[FRONTEND_URL]/subscribe/renew">Gia hạn ngay</a>
                    """,
                    ReplyTo = "support@e-greetings.com"
                });

                processed++;
            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(ex, "[UC15] Failed to process subscription {Id}", sub.Id);
            }
        }

        await _db.SaveChangesAsync();

        await _audit.LogAsync(EventType.JobRun,
            $"[UC15] Auto-disable expired subscriptions: {processed} processed, {errors} errors",
            errors > 0 ? LogStatus.Failed : LogStatus.Success,
            actorType: ActorType.System);

        // Send reminder for subscriptions expiring in 5 and 1 day
        foreach (var daysAhead in BusinessConstants.ReminderDaysBeforeExpiry)
        {
            var reminderDate = now.AddDays(daysAhead).Date;
            var expiringSoon = await _db.Subscriptions
                .Include(s => s.User)
                .Where(s => s.Status == SubscriptionStatus.Active
                         && s.ExpiryDate.HasValue
                         && s.ExpiryDate.Value.Date == reminderDate)
                .ToListAsync();

            foreach (var sub in expiringSoon)
            {
                try
                {
                    await _emailService.SendAsync(new EmailMessage
                    {
                        To = sub.User.Email,
                        Subject = $"Dịch vụ Subscribe sắp hết hạn ({daysAhead} ngày nữa)",
                        HtmlBody = $"<p>Dịch vụ của bạn hết hạn ngày {sub.ExpiryDate:dd/MM/yyyy}. Gia hạn ngay để không bị gián đoạn.</p>",
                        ReplyTo = "support@e-greetings.com"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[UC15] Reminder email failed for sub {Id}", sub.Id);
                }
            }
        }

        _logger.LogInformation("[UC15] Completed. Processed={P}, Errors={E}", processed, errors);
    }
}

/// <summary>
/// UC20 – Send daily greeting cards to all active subscribers at 08:00.
/// BR-23: Random from featured. BR-24: Max 1/day per recipient.
/// </summary>
public class SendDailyGreetingsJob
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;
    private readonly ILogger<SendDailyGreetingsJob> _logger;

    public SendDailyGreetingsJob(
        AppDbContext db, IEmailService emailService,
        IAuditLogService audit, ILogger<SendDailyGreetingsJob> logger)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
        _logger = logger;
    }

    [DisableConcurrentExecution(60 * 60)]
    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        _logger.LogInformation("[UC20] SendDailyGreetings started at {Time}", now);

        var activeSubscriptions = await _db.Subscriptions
            .Include(s => s.User)
            .Include(s => s.EmailList)
            .Where(s => s.Status == SubscriptionStatus.Active)
            .ToListAsync();

        // BR-23: Get featured cards (fallback to all active if no featured)
        var featuredCards = await _db.GreetingCards
            .Where(c => c.IsFeatured && c.Status == CardStatus.Active && !c.IsDeleted)
            .ToListAsync();

        if (!featuredCards.Any())
        {
            featuredCards = await _db.GreetingCards
                .Where(c => c.Status == CardStatus.Active && !c.IsDeleted)
                .ToListAsync();
        }

        if (!featuredCards.Any())
        {
            _logger.LogWarning("[UC20] No active cards found. Skipping.");
            return;
        }

        var rng = new Random();
        int totalSent = 0, totalErrors = 0;

        foreach (var sub in activeSubscriptions)
        {
            var card = featuredCards[rng.Next(featuredCards.Count)];

            foreach (var emailEntry in sub.EmailList)
            {
                // BR-24: Check if already sent today to this email for this subscription
                var alreadySent = await _db.GreetingTransactions.AnyAsync(t =>
                    t.SubscriptionId == sub.Id &&
                    t.RecipientEmail == emailEntry.Email &&
                    t.CreatedAt >= today && t.CreatedAt < today.AddDays(1));

                if (alreadySent) continue;

                // BR-12: Create transaction record
                var transaction = new EGreetings.Domain.Entities.GreetingTransaction
                {
                    SenderId = sub.UserId,
                    CardId = card.Id,
                    RecipientEmail = emailEntry.Email,
                    Subject = $"Thiệp hàng ngày từ E-Greetings – {card.Name}",
                    SubscriptionId = sub.Id,
                    Status = TransactionStatus.Pending
                };

                await _db.GreetingTransactions.AddAsync(transaction);
                await _db.SaveChangesAsync();

                try
                {
                    // BR-28: ReplyTo = user's email
                    await _emailService.SendAsync(new EmailMessage
                    {
                        To = emailEntry.Email,
                        Subject = transaction.Subject,
                        HtmlBody = $"""
                            <div style="font-family:sans-serif;">
                                <h2>Thiệp Hàng Ngày – {card.Name}</h2>
                                <img src="{card.ThumbnailUrl}" alt="{card.Name}" style="max-width:600px;" />
                                <p>Chúc bạn một ngày tốt lành!</p>
                            </div>
                        """,
                        ReplyTo = sub.User.Email  // BR-28
                    });

                    transaction.Status = TransactionStatus.Sent;
                    transaction.SentAt = DateTime.UtcNow;
                    totalSent++;
                }
                catch (Exception ex)
                {
                    transaction.Status = TransactionStatus.Failed;
                    totalErrors++;

                    // BR-32: Add to retry queue
                    await _db.EmailRetryQueues.AddAsync(new EGreetings.Domain.Entities.EmailRetryQueue
                    {
                        GreetingTransactionId = transaction.Id,
                        NextRetryAt = DateTime.UtcNow.AddMinutes(BusinessConstants.EmailRetryIntervalMinutes),
                        LastError = ex.Message,
                        Status = RetryStatus.PendingRetry
                    });

                    _logger.LogError(ex, "[UC20] Failed to send to {Email}", emailEntry.Email);
                }

                await _db.SaveChangesAsync();
            }
        }

        await _audit.LogAsync(EventType.JobRun,
            $"[UC20] Daily greeting: {totalSent} sent, {totalErrors} failed",
            totalErrors > totalSent / 2 ? LogStatus.Failed : LogStatus.Success,
            actorType: ActorType.System);

        _logger.LogInformation("[UC20] Done. Sent={S}, Errors={E}", totalSent, totalErrors);
    }
}

/// <summary>
/// UC29 – Retry failed email sends.
/// BR-32: Max 3 retries, 5 min interval. After 3 failures: alert Admin.
/// Runs every 5 minutes.
/// </summary>
public class RetryFailedEmailsJob
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;
    private readonly ILogger<RetryFailedEmailsJob> _logger;

    public RetryFailedEmailsJob(
        AppDbContext db, IEmailService emailService,
        IAuditLogService audit, ILogger<RetryFailedEmailsJob> logger)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;

        var pendingRetries = await _db.EmailRetryQueues
            .Include(q => q.GreetingTransaction)
                .ThenInclude(t => t.Sender)
            .Include(q => q.GreetingTransaction)
                .ThenInclude(t => t.Card)
            .Where(q => q.Status == RetryStatus.PendingRetry && q.NextRetryAt <= now)
            .ToListAsync();

        foreach (var item in pendingRetries)
        {
            var tx = item.GreetingTransaction;

            try
            {
                // BR-32: Max 3 retries
                if (item.RetryCount >= BusinessConstants.MaxEmailRetryAttempts)
                {
                    item.Status = RetryStatus.Failed;
                    tx.Status = TransactionStatus.Failed;

                    // Alert Admin
                    await _audit.LogAsync(EventType.SystemError,
                        $"[UC29] Email permanently failed after {BusinessConstants.MaxEmailRetryAttempts} retries: {tx.RecipientEmail}",
                        LogStatus.Failed, actorType: ActorType.System);

                    _logger.LogError("[UC29] Permanently failed: {Email} (TxId={TxId})",
                        tx.RecipientEmail, tx.Id);
                }
                else
                {
                    // BR-28: ReplyTo = sender's email
                    await _emailService.SendAsync(new EmailMessage
                    {
                        To = tx.RecipientEmail,
                        Subject = tx.Subject,
                        HtmlBody = $"<p>{tx.PersonalMessage}</p><p>Thiệp: {tx.Card.Name}</p>",
                        ReplyTo = tx.Sender.Email
                    });

                    item.Status = RetryStatus.Sent;
                    tx.Status = TransactionStatus.Sent;
                    tx.SentAt = now;

                    _logger.LogInformation("[UC29] Retry success: {Email} (attempt {N})",
                        tx.RecipientEmail, item.RetryCount + 1);
                }
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.LastError = ex.Message;
                item.NextRetryAt = now.AddMinutes(BusinessConstants.EmailRetryIntervalMinutes);
                item.UpdatedAt = now;

                _logger.LogWarning(ex, "[UC29] Retry {N} failed: {Email}", item.RetryCount, tx.RecipientEmail);
            }
        }

        if (pendingRetries.Any())
            await _db.SaveChangesAsync();
    }
}
