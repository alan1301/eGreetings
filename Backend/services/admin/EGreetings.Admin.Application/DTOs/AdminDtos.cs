using EGreetings.Admin.Domain.Enums;

namespace EGreetings.Admin.Application.DTOs;

public record AuditLogDto(
    int Id,
    AuditEventType EventType,
    string EntityName,
    int? EntityId,
    string Description,
    int? UserId,
    string? IpAddress,
    bool IsSystemAction,
    DateTime OccurredAt);

public record WebContentDto(
    int Id,
    string Key,
    string Title,
    string Content,
    string? ImageUrl,
    int Version,
    bool IsActive,
    int? UpdatedByUserId,
    DateTime CreatedAt);

public record WebContentVersionDto(
    int Id,
    int WebContentId,
    string Title,
    int Version,
    DateTime? ArchivedAt);

public record DashboardDto(
    int TotalUsers,
    int ActiveSubscriptions,
    int PendingPayments,
    int GreetingsSentToday,
    int UnreadFeedbacks,
    decimal RevenueThisMonth);

public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public PagedResult(List<T> items, int page, int pageSize, int totalCount)
        : this(items, page, pageSize, totalCount, (totalCount + pageSize - 1) / pageSize)
    {
    }
}
