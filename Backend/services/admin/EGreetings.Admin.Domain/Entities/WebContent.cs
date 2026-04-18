using EGreetings.Shared.Domain;

namespace EGreetings.Admin.Domain.Entities;

public class WebContent : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int? UpdatedByUserId { get; set; }

    public ICollection<WebContentVersion> Versions { get; set; } = new List<WebContentVersion>();
}
