namespace EGreetings.Domain.Entities;

/// <summary>
/// UC05, UC17 – Auto-saved draft.
/// BR-09: Auto-save every 30s. Max 50 drafts per user (enforced in Application layer).
/// </summary>
public class Draft : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid CardId { get; set; }
    public string? PersonalMessage { get; set; }           // BR-08: max 500 chars
    public string? CustomJsonContent { get; set; }         // JSON: positions, sizes, elements
    public DateTime LastAutoSavedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public GreetingCard Card { get; set; } = null!;
}
