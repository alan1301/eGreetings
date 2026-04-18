using EGreetings.Shared.Domain;

namespace EGreetings.Admin.Domain.Entities;

public class WebContentVersion : BaseEntity
{
    public int WebContentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Version { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public WebContent? WebContent { get; set; }
}
