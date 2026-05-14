namespace EGreetings.Domain.Entities;

/// <summary>
/// A card decoration set (emoji overlay) selectable in the Personalize editor.
/// Id is a slug string (e.g. "bday-balloons") matching the frontend decorId reference.
/// Elements is a JSON array of DecoElement objects stored as a string column.
/// </summary>
public class CardDecoration
{
    public string Id { get; set; } = string.Empty;         // slug, PK
    public string Label { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;    // representative emoji
    public string Categories { get; set; } = "[]";         // JSON array
    public string Elements { get; set; } = "[]";           // JSON array of DecoElement
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
