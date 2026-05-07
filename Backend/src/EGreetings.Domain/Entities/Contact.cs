using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC16 – Contact address book.
/// BR-20: Max 200 contacts per user. BR-21: Name <= 100 chars, email must be valid.
/// </summary>
public class Contact : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;       // BR-21: max 100 chars
    public string Email { get; set; } = string.Empty;      // BR-21: must be valid
    public ContactGroup Group { get; set; } = ContactGroup.Friends;

    /// <summary>Annual occasion date (month/day only — year is ignored). Used for Upcoming Events.</summary>
    public DateOnly? OccasionDate { get; set; }

    /// <summary>Label for the occasion, e.g. "Birthday", "Wedding Anniversary".</summary>
    public string? OccasionLabel { get; set; }

    public User User { get; set; } = null!;
}
