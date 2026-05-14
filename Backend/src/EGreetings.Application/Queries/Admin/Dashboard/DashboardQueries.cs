using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Admin.Dashboard;

// ──────── /overview ────────────────────────────────────────────────────────
public record GetDashboardOverviewQuery() : IRequest<DashboardOverviewDto>;

public class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    private readonly IAppDbContext _db;
    public GetDashboardOverviewQueryHandler(IAppDbContext db) => _db = db;

    public async Task<DashboardOverviewDto> Handle(GetDashboardOverviewQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var weekStart = today.AddDays(-6);
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        var sentToday = await _db.GreetingTransactions.CountAsync(t => t.CreatedAt >= today, ct);
        var sentYesterday = await _db.GreetingTransactions
            .CountAsync(t => t.CreatedAt >= yesterday && t.CreatedAt < today, ct);
        var activeSubs = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Active, ct);
        var pendingPayments = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Pending, ct);
        var unreadFeedbacks = await _db.Feedbacks.CountAsync(f => f.Status == FeedbackStatus.Unread, ct);
        var totalUsers = await _db.Users.CountAsync(u => !u.IsDeleted, ct);

        var revenueThisMonth = await _db.PaymentTransactions
            .Where(p => p.Status == PaymentStatus.Paid && p.PaidAt >= monthStart)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        var revenueLastMonth = await _db.PaymentTransactions
            .Where(p => p.Status == PaymentStatus.Paid && p.PaidAt >= lastMonthStart && p.PaidAt < monthStart)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        // Sparkline 7 days
        var sentByDayRaw = await _db.GreetingTransactions
            .Where(t => t.CreatedAt >= weekStart)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var sentSpark = BuildSparkline(weekStart, 7, sentByDayRaw.ToDictionary(x => x.Date, x => x.Count));

        var pendingSubsByDayRaw = await _db.Subscriptions
            .Where(s => s.CreatedAt >= weekStart && s.Status == SubscriptionStatus.Pending)
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var pendingSpark = BuildSparkline(weekStart, 7, pendingSubsByDayRaw.ToDictionary(x => x.Date, x => x.Count));

        var activeSubsByDayRaw = await _db.Subscriptions
            .Where(s => s.StartDate != null && s.StartDate >= weekStart)
            .GroupBy(s => s.StartDate!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var activeSpark = BuildSparkline(weekStart, 7, activeSubsByDayRaw.ToDictionary(x => x.Date, x => x.Count));

        var feedbackByDayRaw = await _db.Feedbacks
            .Where(f => f.CreatedAt >= weekStart)
            .GroupBy(f => f.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var feedbackSpark = BuildSparkline(weekStart, 7, feedbackByDayRaw.ToDictionary(x => x.Date, x => x.Count));

        return new DashboardOverviewDto(
            sentToday, sentYesterday, activeSubs, pendingPayments, unreadFeedbacks, totalUsers,
            revenueThisMonth, revenueLastMonth,
            sentSpark, activeSpark, pendingSpark, feedbackSpark);
    }

    private static IReadOnlyList<int> BuildSparkline(DateTime start, int days, Dictionary<DateTime, int> data)
    {
        var result = new int[days];
        for (int i = 0; i < days; i++)
        {
            var date = start.AddDays(i).Date;
            result[i] = data.TryGetValue(date, out var v) ? v : 0;
        }
        return result;
    }
}

// ──────── /timeseries ──────────────────────────────────────────────────────
public record GetDashboardTimeseriesQuery(string Range = "7d") : IRequest<TimeseriesDto>;

public class GetDashboardTimeseriesQueryHandler : IRequestHandler<GetDashboardTimeseriesQuery, TimeseriesDto>
{
    private readonly IAppDbContext _db;
    public GetDashboardTimeseriesQueryHandler(IAppDbContext db) => _db = db;

    public async Task<TimeseriesDto> Handle(GetDashboardTimeseriesQuery request, CancellationToken ct)
    {
        var days = request.Range switch { "30d" => 30, "today" => 1, _ => 7 };
        var today = DateTime.UtcNow.Date;
        var start = today.AddDays(-(days - 1));

        var sentRaw = await _db.GreetingTransactions
            .Where(t => t.CreatedAt >= start)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var sentDict = sentRaw.ToDictionary(x => x.Date, x => x.Count);

        var revRaw = await _db.PaymentTransactions
            .Where(p => p.Status == PaymentStatus.Paid && p.PaidAt != null && p.PaidAt >= start)
            .GroupBy(p => p.PaidAt!.Value.Date)
            .Select(g => new { Date = g.Key, Sum = g.Sum(x => x.Amount) })
            .ToListAsync(ct);
        var revDict = revRaw.ToDictionary(x => x.Date, x => x.Sum);

        var points = new List<TimeseriesPointDto>(days);
        for (int i = 0; i < days; i++)
        {
            var date = start.AddDays(i);
            points.Add(new TimeseriesPointDto(
                date,
                sentDict.TryGetValue(date, out var c) ? c : 0,
                revDict.TryGetValue(date, out var r) ? r : 0m));
        }
        return new TimeseriesDto(request.Range, points);
    }
}

// ──────── /funnel ──────────────────────────────────────────────────────────
public record GetSubscriptionFunnelQuery() : IRequest<FunnelDto>;

public class GetSubscriptionFunnelQueryHandler : IRequestHandler<GetSubscriptionFunnelQuery, FunnelDto>
{
    private readonly IAppDbContext _db;
    public GetSubscriptionFunnelQueryHandler(IAppDbContext db) => _db = db;

    public async Task<FunnelDto> Handle(GetSubscriptionFunnelQuery request, CancellationToken ct)
    {
        var pending = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Pending, ct);
        var active = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Active, ct);
        var expired = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Expired, ct);
        var disabled = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Disabled, ct);
        var total = pending + active + expired + disabled;
        var conversion = total == 0 ? 0 : (double)(active + expired) / total * 100;
        return new FunnelDto(pending, active, expired, disabled, Math.Round(conversion, 1));
    }
}

// ──────── /top-cards ───────────────────────────────────────────────────────
public record GetTopCardsQuery(string Range = "7d", int Take = 5) : IRequest<IReadOnlyList<TopCardDto>>;

public class GetTopCardsQueryHandler : IRequestHandler<GetTopCardsQuery, IReadOnlyList<TopCardDto>>
{
    private readonly IAppDbContext _db;
    public GetTopCardsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<TopCardDto>> Handle(GetTopCardsQuery request, CancellationToken ct)
    {
        var days = request.Range == "30d" ? 30 : 7;
        var take = Math.Clamp(request.Take, 1, 20);
        var start = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var top = await _db.GreetingTransactions
            .Where(t => t.CreatedAt >= start)
            .GroupBy(t => new { t.CardId, t.Card.Name, t.Card.ThumbnailUrl })
            .Select(g => new { g.Key.CardId, g.Key.Name, g.Key.ThumbnailUrl, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync(ct);

        var ids = top.Select(x => x.CardId).ToList();
        var perDay = await _db.GreetingTransactions
            .Where(t => t.CreatedAt >= start && ids.Contains(t.CardId))
            .GroupBy(t => new { t.CardId, t.CreatedAt.Date })
            .Select(g => new { g.Key.CardId, g.Key.Date, Count = g.Count() })
            .ToListAsync(ct);

        return top.Select(t =>
        {
            var spark = new int[days];
            foreach (var p in perDay.Where(x => x.CardId == t.CardId))
            {
                var idx = (p.Date - start).Days;
                if (idx >= 0 && idx < days) spark[idx] = p.Count;
            }
            return new TopCardDto(t.CardId, t.Name, t.ThumbnailUrl, t.Count, spark);
        }).ToList();
    }
}

// ──────── /activity ────────────────────────────────────────────────────────
public record GetRecentActivityQuery(int Take = 20) : IRequest<IReadOnlyList<ActivityItemDto>>;

public class GetRecentActivityQueryHandler : IRequestHandler<GetRecentActivityQuery, IReadOnlyList<ActivityItemDto>>
{
    private readonly IAppDbContext _db;
    public GetRecentActivityQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ActivityItemDto>> Handle(GetRecentActivityQuery request, CancellationToken ct)
    {
        var take = Math.Clamp(request.Take, 1, 50);

        var greetings = await _db.GreetingTransactions
            .Include(t => t.Sender)
            .Include(t => t.Card)
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .Select(t => new ActivityItemDto(
                "g_" + t.Id.ToString(),
                "GreetingSent",
                t.Card.Name,
                "to " + t.RecipientEmail,
                t.Sender.FullName,
                "",
                t.CreatedAt,
                t.Status.ToString(),
                t.Id,
                "view"))
            .ToListAsync(ct);

        var subs = await _db.Subscriptions
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .Take(take)
            .Select(s => new ActivityItemDto(
                "s_" + s.Id.ToString(),
                s.Status == SubscriptionStatus.Pending ? "PaymentPending" : "SubscriptionCreated",
                s.Plan.ToString() + " subscription",
                s.Status.ToString(),
                s.User.FullName,
                "",
                s.CreatedAt,
                s.Status.ToString(),
                s.Id,
                s.Status == SubscriptionStatus.Pending ? "approve" : null))
            .ToListAsync(ct);

        var feedbacks = await _db.Feedbacks
            .Include(f => f.User)
            .Where(f => f.Status == FeedbackStatus.Unread)
            .OrderByDescending(f => f.CreatedAt)
            .Take(take)
            .Select(f => new ActivityItemDto(
                "f_" + f.Id.ToString(),
                "FeedbackSubmitted",
                f.Title,
                f.StarRating != null ? f.StarRating + "★" : null,
                f.User.FullName,
                "",
                f.CreatedAt,
                f.Status.ToString(),
                f.Id,
                "mark-read"))
            .ToListAsync(ct);

        var logs = await _db.SystemLogs
            .Where(l => l.EventType == EventType.AdminAction || l.EventType == EventType.SystemError
                     || l.Status == LogStatus.Failed)
            .OrderByDescending(l => l.Timestamp)
            .Take(take)
            .Select(l => new ActivityItemDto(
                "l_" + l.Id.ToString(),
                l.EventType.ToString(),
                l.Description,
                l.IpAddress,
                l.ActorType.ToString(),
                "",
                l.Timestamp,
                l.Status.ToString(),
                (Guid?)null,
                (string?)null))
            .ToListAsync(ct);

        var all = new List<ActivityItemDto>();
        all.AddRange(greetings);
        all.AddRange(subs);
        all.AddRange(feedbacks);
        all.AddRange(logs);

        return all
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .Select(x => x with { ActorInitials = MakeInitials(x.ActorName) })
            .ToList();
    }

    private static string MakeInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
    }
}

// ──────── /health ──────────────────────────────────────────────────────────
public record GetSystemHealthQuery() : IRequest<HealthDto>;

public class GetSystemHealthQueryHandler : IRequestHandler<GetSystemHealthQuery, HealthDto>
{
    private static readonly DateTime _startedAt = DateTime.UtcNow;
    private readonly IAppDbContext _db;
    public GetSystemHealthQueryHandler(IAppDbContext db) => _db = db;

    public async Task<HealthDto> Handle(GetSystemHealthQuery request, CancellationToken ct)
    {
        string dbStatus;
        try
        {
            _ = await _db.Users.CountAsync(ct);
            dbStatus = "ok";
        }
        catch
        {
            dbStatus = "down";
        }

        var queueDepth = await _db.EmailRetryQueues
            .CountAsync(r => r.Status == RetryStatus.PendingRetry, ct);

        var lastJob = await _db.SystemLogs
            .Where(l => l.EventType == EventType.JobRun)
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new { l.Timestamp, l.Status })
            .FirstOrDefaultAsync(ct);

        var uptime = (long)(DateTime.UtcNow - _startedAt).TotalSeconds;
        return new HealthDto(
            dbStatus,
            queueDepth,
            lastJob?.Timestamp,
            lastJob?.Status.ToString() ?? "Unknown",
            uptime);
    }
}
