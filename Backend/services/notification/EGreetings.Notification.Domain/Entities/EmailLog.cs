namespace EGreetings.Notification.Domain.Entities;

public enum EmailLogStatus { Pending, Sent, Failed }

/// <summary>
/// UC29 - Retry Email - BR-32: Tối đa 3 lần retry, cách 5 phút
/// </summary>
public class EmailLog
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int? GreetingId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailLogStatus Status { get; set; } = EmailLogStatus.Pending;
    public int RetryCount { get; set; } = 0;       // BR-32: tối đa 3 lần
    public DateTime? LastRetryAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}
