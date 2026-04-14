using EGreetings.Domain.Common;
using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC30 - Logging & Audit - BR-33: Immutable, lưu 30 ngày, hỗ trợ export CSV
/// </summary>
public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }          // null nếu là System action
    public AuditEventType EventType { get; set; }
    public string EntityName { get; set; } = string.Empty;   // "User", "Greeting", ...
    public string? EntityId { get; set; }
    public string? OldValue { get; set; }    // JSON serialized
    public string? NewValue { get; set; }    // JSON serialized
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsSystemAction { get; set; } = false;

    // Navigation
    public User? User { get; set; }
}
