using EGreetings.Admin.Domain.Enums;
using EGreetings.Shared.Domain;

namespace EGreetings.Admin.Domain.Entities;

public class AuditLog : BaseEntity
{
    public AuditEventType EventType { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSystemAction { get; set; }
    public DateTime OccurredAt { get; set; }
}
