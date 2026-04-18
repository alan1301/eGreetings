using EGreetings.Shared.Domain;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Domain.Entities;

public class Subscription : BaseEntity
{
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiredAt { get; set; }
    public int MaxRecipients { get; set; }
    public bool IsDisabledByAdmin { get; set; }
    public string? DisabledReason { get; set; }

    // Navigation properties
    public virtual ICollection<SubscriptionRecipient> Recipients { get; set; } = new List<SubscriptionRecipient>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual SubscriptionPlan? Plan { get; set; }
}
