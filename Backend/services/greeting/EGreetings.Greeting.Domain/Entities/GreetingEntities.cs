using EGreetings.Shared.Domain.Common;

namespace EGreetings.Greeting.Domain.Entities;

public enum GreetingStatus { Draft, Scheduled, Sent, Failed }

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public List<GreetingTemplate> Templates { get; set; } = new();
}

public class GreetingTemplate : BaseEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsFree { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; }
}

public class GreetingCard : BaseEntity
{
    public int? UserId { get; set; }
    public int TemplateId { get; set; }
    public GreetingTemplate Template { get; set; } = null!;
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? SenderMessage { get; set; }
    public string? GuestSenderName { get; set; }
    public string? GuestSenderEmail { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? CustomHtml { get; set; }
    public string UniqueToken { get; set; } = Guid.NewGuid().ToString("N");
    public GreetingStatus Status { get; set; } = GreetingStatus.Draft;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int ViewCount { get; set; }
}
