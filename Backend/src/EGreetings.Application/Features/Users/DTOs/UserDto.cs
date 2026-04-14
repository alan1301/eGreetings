namespace EGreetings.Application.Features.Users.DTOs;

public record UserProfileDto(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    string? AvatarUrl,
    string Role,
    string Status,
    DateTime? EmailVerifiedAt,
    DateTime CreatedAt,
    // UC19: Trạng thái Subscribe (A3 flow)
    string? SubscribeStatus,
    DateTime? SubscribeExpiredAt
);

public record UserListItemDto(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    string Role,
    string Status,
    DateTime? EmailVerifiedAt,
    DateTime CreatedAt
);

public record ContactDto(
    int Id,
    int UserId,
    string Name,
    string Email,
    string? Phone,
    DateTime? Birthday,
    string? Note,
    string? Group,       // UC16: Gia đình / Bạn bè / Đồng nghiệp
    DateTime CreatedAt
);

public record DraftDto(
    int Id,
    int UserId,
    int TemplateId,
    string TemplateName,
    string Title,
    string? RecipientEmail,
    string? RecipientName,
    string? SenderMessage,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize
);
