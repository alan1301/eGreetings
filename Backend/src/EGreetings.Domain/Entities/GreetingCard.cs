using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC09, UC10, UC03, UC04, UC24 – Greeting card template.
/// BR-17: Only Admin manages. BR-18: No hard delete if has transactions. BR-19: Archive = Inactive.
/// BR-23: IsFeatured cards used for Subscribe daily send.
/// </summary>
public class GreetingCard : BaseAuditableEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Tags { get; set; }                      // comma-separated
    public string? ThumbnailUrl { get; set; }
    public string? FileUrl { get; set; }
    public string? CustomJsonContent { get; set; }
    public string? Description { get; set; }
    public bool IsFeatured { get; set; } = false;          // BR-23
    public bool IsPremium { get; set; } = true;
    public CardStatus Status { get; set; } = CardStatus.Active;

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<GreetingTransaction> Transactions { get; set; } = new List<GreetingTransaction>();
    public ICollection<Draft> Drafts { get; set; } = new List<Draft>();
}
