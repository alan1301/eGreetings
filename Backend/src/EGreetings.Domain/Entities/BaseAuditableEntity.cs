using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// Base class for all soft-deletable entities with audit fields.
/// </summary>
public abstract class BaseAuditableEntity : HasDomainEvents
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
