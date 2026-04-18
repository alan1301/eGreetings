namespace EGreetings.Subscription.Application.DTOs;

public record SubscriptionDto(
    int Id,
    int UserId,
    string PlanName,
    string Status,
    decimal Price,
    DateTime StartDate,
    DateTime ExpiredAt,
    int MaxRecipients,
    int RecipientCount,
    DateTime CreatedAt
);

public record SubscriptionDetailDto(
    int Id,
    int UserId,
    string PlanName,
    string Status,
    decimal Price,
    DateTime StartDate,
    DateTime ExpiredAt,
    int MaxRecipients,
    int RecipientCount,
    DateTime CreatedAt,
    List<SubscriptionRecipientDto> Recipients
);

public record SubscriptionRecipientDto(
    int Id,
    string Email,
    string Name,
    DateTime? Birthday,
    string? Occasion,
    bool IsActive
);

public record PaymentDto(
    int Id,
    int SubscriptionId,
    decimal Amount,
    string Status,
    string Method,
    string? TransactionCode,
    DateTime? PaidAt,
    DateTime CreatedAt
);

public record CreateSubscriptionResponse(
    int SubscriptionId,
    int PaymentId,
    decimal Amount,
    string Message
);

public record PlanDto(
    int Id,
    string Name,
    string? Description,
    int MaxRecipients,
    decimal PricePerYear,
    bool IsActive
);
