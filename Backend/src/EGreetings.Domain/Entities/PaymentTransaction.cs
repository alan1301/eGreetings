using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC08, UC13, UC25, UC26 – Payment transaction record.
/// Immutable – never delete.
/// </summary>
public class PaymentTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod PaymentMethod { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayProvider { get; set; }           // VNPay, MoMo, PayPal
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Subscription Subscription { get; set; } = null!;
}
