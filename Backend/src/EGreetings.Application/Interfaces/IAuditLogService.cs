using EGreetings.Domain.Enums;

namespace EGreetings.Application.Interfaces;

/// <summary>
/// UC30 – Audit log service. Append-only (BR-33).
/// </summary>
public interface IAuditLogService
{
    Task LogAsync(
        EventType eventType,
        string description,
        LogStatus status = LogStatus.Success,
        Guid? actorId = null,
        ActorType actorType = ActorType.System,
        string? ipAddress = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default);
}
