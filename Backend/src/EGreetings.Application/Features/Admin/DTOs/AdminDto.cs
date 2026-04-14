// UserListItemDto lives in EGreetings.Application.Features.Users.DTOs — imported there

namespace EGreetings.Application.Features.Admin.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    string? IconUrl,
    bool IsSystem,
    bool IsActive,
    int DisplayOrder,
    int TemplateCount,
    DateTime CreatedAt
);

public record AuditLogDto(
    int Id,
    string EventType,
    string EntityName,
    string? EntityId,
    string? Description,
    int? UserId,
    string? IpAddress,
    bool IsSystemAction,
    DateTime CreatedAt
);

public record DashboardStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalGreetings,
    int SentGreetings,
    int ActiveSubscriptions,
    decimal TotalRevenue,
    int TotalTemplates,
    int UnreadFeedbacks
);

public record WebContentVersionDto(
    int Id,
    string Key,
    string Title,
    int Version,
    bool IsActive,
    int? UpdatedByUserId,
    DateTime CreatedAt
);

public record FeedbackDto(
    int Id,
    int? UserId,
    string Subject,
    string Content,
    string? ContactEmail,
    int? StarRating,     // UC07: Đánh giá sao 1–5 (tùy chọn)
    bool IsRead,
    string? AdminReply,
    DateTime? RepliedAt,
    DateTime CreatedAt
);

public record PaymentDto(
    int Id,
    int? UserId,
    int? SubscriptionId,
    string TransactionCode,
    decimal Amount,
    string Currency,
    string Status,
    string? PaymentMethod,
    DateTime? PaidAt,
    string? Note,
    DateTime CreatedAt,
    // UC25: Ngày hết hạn Subscribe liên kết với payment này
    DateTime? SubscriptionExpiredAt
);
