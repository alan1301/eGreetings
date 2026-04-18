using EGreetings.Shared.Domain;

namespace EGreetings.Subscription.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public required string Name { get; set; }
    public int MaxRecipients { get; set; }
    public decimal PricePerYear { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
