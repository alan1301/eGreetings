using EGreetings.Domain.Common;
using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC06, UC20, UC17 – Immutable transaction log for every greeting sent.
/// BR-12: Must always be saved. NEVER delete this record.
/// </summary>
public class GreetingTransaction : HasDomainEvents
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenderId { get; set; }
    public Guid CardId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? PersonalMessage { get; set; }
    public DateTime? ScheduledSendAt { get; set; }         // BR-11: >= UtcNow + 5min
    public DateTime? SentAt { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public int RetryCount { get; set; } = 0;               // BR-32
    public Guid? SubscriptionId { get; set; }              // null = manual send (UC06), set = auto (UC20)
    public bool IsFresh { get; set; } = false;             // true = user composed their own design (not bound to template content)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Sender { get; set; } = null!;
    public GreetingCard Card { get; set; } = null!;
    public Subscription? Subscription { get; set; }
    public EmailRetryQueue? RetryQueue { get; set; }
}
