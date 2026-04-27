using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC07, UC11 – User feedback.
/// BR-13: Max 5 feedbacks per user per day.
/// </summary>
public class Feedback
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? StarRating { get; set; }                   // 1-5, optional
    public FeedbackStatus Status { get; set; } = FeedbackStatus.Unread;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
