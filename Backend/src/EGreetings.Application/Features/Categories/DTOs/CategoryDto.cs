namespace EGreetings.Application.Features.Categories.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    string? IconUrl,
    bool IsActive,
    int TemplateCount,
    DateTime CreatedAt
);
