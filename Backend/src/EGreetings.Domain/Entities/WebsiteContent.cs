namespace EGreetings.Domain.Entities;

/// <summary>
/// UC28 – CMS for website content (banner, footer, about).
/// BR-17: Only Admin can edit. Keeps last 10 versions (implemented as separate records).
/// </summary>
public class WebsiteContent : BaseAuditableEntity
{
    public string Section { get; set; } = string.Empty;    // "banner", "footer", "about"
    public string Key { get; set; } = string.Empty;        // "title", "description", "imageUrl"
    public string Value { get; set; } = string.Empty;
    public string? ContentType { get; set; }               // "text", "html", "url"
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
