using EGreetings.Shared.Domain;

namespace EGreetings.Greeting.Domain.Entities;

public class GreetingTemplate : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public string? CssStyle { get; set; }
    public bool IsFree { get; set; }
    public bool IsActive { get; set; } = true;
    public long UsageCount { get; set; }
}
