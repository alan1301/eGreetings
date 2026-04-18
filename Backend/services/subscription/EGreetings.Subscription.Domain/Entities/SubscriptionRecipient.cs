using EGreetings.Shared.Domain;

namespace EGreetings.Subscription.Domain.Entities;

public class SubscriptionRecipient : BaseEntity
{
    public int SubscriptionId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Occasion { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Subscription? Subscription { get; set; }
}
