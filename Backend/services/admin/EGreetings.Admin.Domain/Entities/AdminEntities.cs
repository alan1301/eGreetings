using EGreetings.Shared.Domain.Common;

namespace EGreetings.Admin.Domain.Entities;

public enum AuditEventType { UserBanned, UserUnbanned, CategoryCreated, CategoryDeleted, WebContentUpdated, GreetingSent, AutoSendTriggered }

public class AuditLog : BaseEntity
{
    public AuditEventType EventType { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSystemAction { get; set; }
}

public class WebContent : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public int? PreviousVersionId { get; set; }
    public int? UpdatedByUserId { get; set; }
}
