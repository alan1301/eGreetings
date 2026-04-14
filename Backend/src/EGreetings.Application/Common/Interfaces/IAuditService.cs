using EGreetings.Domain.Enums;

namespace EGreetings.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditEventType eventType, string entityName, string? entityId,
        string description, string? oldValue = null, string? newValue = null,
        bool isSystemAction = false, CancellationToken cancellationToken = default);
}
