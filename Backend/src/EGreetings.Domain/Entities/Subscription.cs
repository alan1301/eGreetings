using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC08, UC13, UC14, UC15, UC26 – Subscription service.
/// State machine: Pending → Active → Expired / Disabled.
/// BR-15: Only activate after payment confirmed. BR-16: Auto-disable when expired.
/// </summary>
public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    public DateTime? StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? DisabledReason { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Gateway;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<SubscriptionEmailList> EmailList { get; set; } = new List<SubscriptionEmailList>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<GreetingTransaction> GreetingTransactions { get; set; } = new List<GreetingTransaction>();
}
