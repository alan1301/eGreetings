using EGreetings.Greeting.Domain.Enums;
using EGreetings.Shared.Domain;

namespace EGreetings.Greeting.Domain.Entities;

public class Greeting : BaseEntity
{
    public int? UserId { get; set; }
    public int TemplateId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? SenderMessage { get; set; }
    public string? CustomHtml { get; set; }
    public string ViewToken { get; set; } = string.Empty;
    public GreetingStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }

    // Navigation
    public GreetingTemplate? Template { get; set; }
}
