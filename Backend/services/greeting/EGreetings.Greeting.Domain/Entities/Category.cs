using EGreetings.Shared.Domain;

namespace EGreetings.Greeting.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
