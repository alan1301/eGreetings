using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.Services;

/// <summary>
/// UC30 – Append-only system audit log (BR-33: never update/delete).
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AppDbContext db, ILogger<AuditLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(
        EventType eventType,
        string description,
        LogStatus status = LogStatus.Success,
        Guid? actorId = null,
        ActorType actorType = ActorType.System,
        string? ipAddress = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new EGreetings.Domain.Entities.SystemLog
            {
                Timestamp = DateTime.UtcNow,
                ActorId = actorId,
                ActorType = actorType,
                EventType = eventType,
                Description = description?.Length > 1000
                    ? description[..1000]
                    : description ?? string.Empty,
                Status = status,
                IpAddress = ipAddress,
                AdditionalData = additionalData,
                CreatedAt = DateTime.UtcNow
            };

            // BR-33: ONLY Add – never Update or Delete
            await _db.SystemLogs.AddAsync(log, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log to file so we don't lose audit trail even if DB fails
            _logger.LogError(ex, "[AUDIT_FAIL] Could not write audit log: {EventType} – {Description}",
                eventType, description);
        }
    }
}
