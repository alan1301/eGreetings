using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC30 – Append-only system audit log.
/// BR-33: Log >= 5 event groups, retain >= 30 days. NEVER update or delete.
/// </summary>
public class SystemLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;     // Always UTC
    public Guid? ActorId { get; set; }                              // null if System job
    public ActorType ActorType { get; set; } = ActorType.System;
    public EventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public LogStatus Status { get; set; } = LogStatus.Success;
    public string? IpAddress { get; set; }
    public string? AdditionalData { get; set; }                     // JSON extras
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // NOTE: No UpdatedAt – this record is append-only (BR-33)
}
