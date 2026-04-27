using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC27 – Category entity.
/// BR-17: Only Admin can manage. BR-31: No hard delete if it has active cards.
/// Default categories: SINH NHẬT, ĐÁM CƯỚI, NĂM MỚI, LỄ HỘI – cannot be deleted.
/// </summary>
public class Category : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;       // unique, kebab-case
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }                     // hex color
    public int DisplayOrder { get; set; } = 0;
    public bool IsSystem { get; set; } = false;            // BR-31: system categories cannot be deleted
    public CategoryStatus Status { get; set; } = CategoryStatus.Active;

    public ICollection<GreetingCard> Cards { get; set; } = new List<GreetingCard>();
}
