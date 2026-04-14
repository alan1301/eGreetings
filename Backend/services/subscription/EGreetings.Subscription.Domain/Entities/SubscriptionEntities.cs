using EGreetings.Shared.Domain.Common;

namespace EGreetings.Subscription.Domain.Entities;

public enum SubscriptionStatus { Active, Expired, Cancelled }
public enum PaymentStatus { Pending, Paid, Failed }

public class SubscriptionPlan : BaseEntity
{
    public int UserId { get; set; }
    public int RecipientCount { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime ExpiredAt { get; set; }
    public List<SubscriptionRecipient> Recipients { get; set; } = new();
}

public class SubscriptionRecipient : BaseEntity
{
    public int SubscriptionId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class Payment : BaseEntity
{
    public int UserId { get; set; }
    public int SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
}
