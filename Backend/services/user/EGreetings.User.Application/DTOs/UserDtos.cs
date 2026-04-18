namespace EGreetings.User.Application.DTOs;

public record UserProfileDto(
    int Id,
    int UserId,
    string Email,
    string FullName,
    string? Phone,
    string? AvatarUrl,
    DateTime CreatedAt
);

public record ContactDto(
    int Id,
    string Name,
    string Email,
    string? Phone,
    DateTime? Birthday,
    string? Note,
    string? Group,
    DateTime CreatedAt
);

public record DraftDto(
    int Id,
    int TemplateId,
    string? Title,
    string? CustomHtml,
    string? RecipientEmail,
    string? RecipientName,
    string? SenderMessage,
    DateTime CreatedAt
);
