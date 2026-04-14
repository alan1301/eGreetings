namespace EGreetings.Shared.Contracts.Events.Subscription;

public record PaymentConfirmedEvent(
    int PaymentId,
    int UserId,
    int SubscriptionId,
    decimal Amount,
    DateTime PaidAt
);
