namespace EGreetings.Greeting.Application.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    string? IconUrl,
    bool IsActive,
    int TemplateCount,
    DateTime CreatedAt
);

public record TemplateDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string? ThumbnailUrl,
    bool IsFree,
    bool IsActive,
    long UsageCount,
    DateTime CreatedAt
);

public record TemplateDetailDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string? ThumbnailUrl,
    bool IsFree,
    bool IsActive,
    long UsageCount,
    DateTime CreatedAt,
    string? Description,
    string HtmlContent,
    string? CssStyle
);

public record GreetingDto(
    int Id,
    int TemplateId,
    string TemplateName,
    string RecipientEmail,
    string RecipientName,
    string Status,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    string ViewToken,
    DateTime CreatedAt
);

public record SendGreetingResponse(
    int Id,
    string Token,
    string ViewUrl,
    DateTime? ScheduledAt
);
