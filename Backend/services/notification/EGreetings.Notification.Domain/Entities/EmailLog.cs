using EGreetings.Shared.Domain;

namespace EGreetings.Notification.Domain.Entities;

public class EmailLog : BaseEntity
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public int AttemptCount { get; set; } = 0;
    public DateTime? LastAttemptAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? MessageId { get; set; }
}

public enum EmailStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    MaxRetriesExceeded = 3
}
