namespace EGreetings.Application.Features.Admin.DTOs;

public record WebContentDto(
    int Id,
    string Key,
    string Title,
    string Content,
    string? ImageUrl,
    int Version,
    bool IsActive,
    int? UpdatedByUserId,
    DateTime CreatedAt
);
