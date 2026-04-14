using EGreetings.Shared.Domain.Common;

namespace EGreetings.Feedback.Domain.Entities;

public class FeedbackEntry : BaseEntity
{
    public int? UserId { get; set; }
    public string? GuestEmail { get; set; }
    public string Content { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
