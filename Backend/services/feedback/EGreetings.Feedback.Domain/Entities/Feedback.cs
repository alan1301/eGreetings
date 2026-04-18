using EGreetings.Shared.Domain;
using EGreetings.Feedback.Domain.Enums;

namespace EGreetings.Feedback.Domain.Entities;

public class Feedback : BaseEntity
{
    public int? UserId { get; set; }
    public required string Subject { get; set; }
    public required string Content { get; set; }
    public string? ContactEmail { get; set; }
    public int? StarRating { get; set; }
    public FeedbackStatus Status { get; set; }
    public string? Reply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public int? RepliedByAdminId { get; set; }
    public bool IsRead { get; set; }
}
