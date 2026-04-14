namespace EGreetings.Application.Features.Subscriptions.DTOs;

public record SubscriptionDto(
    int Id,
    int UserId,
    string Status,
    decimal Price,
    DateTime StartDate,
    DateTime ExpiredAt,
    int MaxRecipients,
    int CurrentRecipientCount,
    DateTime CreatedAt
);

public record SubscriptionDetailDto(
    int Id,
    int UserId,
    string Status,
    decimal Price,
    DateTime StartDate,
    DateTime ExpiredAt,
    int MaxRecipients,
    int CurrentRecipientCount,
    List<SubscriptionRecipientDto> Recipients,
    DateTime CreatedAt
);

public record SubscriptionRecipientDto(
    int Id,
    string Email,
    string Name,
    DateTime? Birthday,
    string? Occasion,
    bool IsActive
);

public record CreateSubscriptionCommand_Dto(
    decimal Price,
    int DurationDays,
    int MaxRecipients,
    string? PaymentMethod
);

public record RenewSubscriptionDto(
    string? PaymentMethod
);
