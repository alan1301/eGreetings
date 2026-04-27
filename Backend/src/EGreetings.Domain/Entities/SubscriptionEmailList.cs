namespace EGreetings.Domain.Entities;

/// <summary>
/// BR-14: Each subscription must have >= 10 email addresses.
/// </summary>
public class SubscriptionEmailList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubscriptionId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Subscription Subscription { get; set; } = null!;
}
