namespace EGreetings.Application.Features.Greetings.DTOs;

public record GreetingDto(
    int Id,
    int TemplateId,
    string TemplateName,
    string RecipientName,
    string RecipientEmail,
    string SenderMessage,
    string Status,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    int ViewCount,
    string UniqueToken,
    DateTime CreatedAt
);

public record GreetingDetailDto(
    int Id,
    int TemplateId,
    string TemplateName,
    string TemplateThumbnailUrl,
    string CustomHtml,
    string RecipientName,
    string RecipientEmail,
    string SenderMessage,
    string Status,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    int ViewCount,
    string UniqueToken,
    DateTime CreatedAt
);

public record SendGreetingRequest(
    int TemplateId,
    string RecipientEmail,
    string RecipientName,
    string SenderMessage,
    string CustomHtml,
    DateTime? ScheduledAt,
    // UC08 Guest fields
    string? GuestSenderEmail,
    string? GuestSenderName
);
