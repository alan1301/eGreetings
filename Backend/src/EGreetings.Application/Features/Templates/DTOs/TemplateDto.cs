namespace EGreetings.Application.Features.Templates.DTOs;

public record TemplateDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string Description,
    string ThumbnailUrl,
    bool IsFree,
    bool IsActive,
    int UsageCount,
    DateTime CreatedAt
);

public record TemplateDetailDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string Description,
    string ThumbnailUrl,
    string HtmlContent,
    string CssStyle,
    bool IsFree,
    bool IsActive,
    int UsageCount,
    DateTime CreatedAt
);
