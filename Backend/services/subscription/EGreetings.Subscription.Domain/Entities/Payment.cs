using EGreetings.Shared.Domain;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Domain.Entities;

public class Payment : BaseEntity
{
    public int SubscriptionId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public required string Method { get; set; }
    public string? TransactionCode { get; set; }
    public DateTime? PaidAt { get; set; }
    public int? ConfirmedByAdminId { get; set; }

    // Navigation properties
    public virtual Subscription? Subscription { get; set; }
}
