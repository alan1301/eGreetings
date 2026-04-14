using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// Lịch sử giao dịch gửi thiệp (1 thiệp có thể gửi nhiều lần - retry)
/// </summary>
public class GreetingTransaction : BaseEntity
{
    public int GreetingId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Greeting Greeting { get; set; } = null!;
}
