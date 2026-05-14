namespace EGreetings.Domain.Entities;

/// <summary>
/// A card background option selectable in the Personalize editor.
/// Id is a slug string (e.g. "cream", "bday-pink") matching the frontend bgId reference.
/// </summary>
public class CardBackground
{
    public string Id { get; set; } = string.Empty;         // slug, PK
    public string Label { get; set; } = string.Empty;
    public string BgStyle { get; set; } = string.Empty;    // CSS gradient or image URL
    public string Categories { get; set; } = "[]";         // JSON array e.g. ["all","birthday"]
    public bool IsPremium { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
