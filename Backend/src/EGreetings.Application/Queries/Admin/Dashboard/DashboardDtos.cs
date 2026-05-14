namespace EGreetings.Application.Queries.Admin.Dashboard;

public record DashboardOverviewDto(
    int SentToday,
    int SentYesterday,
    int ActiveSubscriptions,
    int PendingPayments,
    int UnreadFeedbacks,
    int TotalUsers,
    decimal RevenueThisMonth,
    decimal RevenueLastMonth,
    IReadOnlyList<int> SentSparkline,
    IReadOnlyList<int> ActiveSubscriptionsSparkline,
    IReadOnlyList<int> PendingPaymentsSparkline,
    IReadOnlyList<int> UnreadFeedbacksSparkline
);

public record TimeseriesPointDto(
    DateTime Date,
    int GreetingsSent,
    decimal Revenue
);

public record TimeseriesDto(
    string Range,
    IReadOnlyList<TimeseriesPointDto> Points
);

public record FunnelDto(
    int Pending,
    int Active,
    int Expired,
    int Disabled,
    double ConversionRate
);

public record TopCardDto(
    Guid CardId,
    string Title,
    string? ThumbnailUrl,
    int SentCount,
    IReadOnlyList<int> Sparkline
);

public record ActivityItemDto(
    string Id,
    string Type,
    string Title,
    string? Subtitle,
    string ActorName,
    string ActorInitials,
    DateTime OccurredAt,
    string Status,
    Guid? TargetId,
    string? Action
);

public record HealthDto(
    string Db,
    int EmailQueueDepth,
    DateTime? SubscribeJobLastRun,
    string SubscribeJobLastStatus,
    long ApiUptimeSeconds
);
