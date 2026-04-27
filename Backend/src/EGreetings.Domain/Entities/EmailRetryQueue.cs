using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC29 – Email retry queue.
/// BR-32: Max 3 retries, 5 min interval. After 3 failures: mark Failed + alert Admin.
/// </summary>
public class EmailRetryQueue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GreetingTransactionId { get; set; }
    public int RetryCount { get; set; } = 0;               // BR-32: max 3
    public DateTime NextRetryAt { get; set; }
    public string? LastError { get; set; }
    public RetryStatus Status { get; set; } = RetryStatus.PendingRetry;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public GreetingTransaction GreetingTransaction { get; set; } = null!;
}
