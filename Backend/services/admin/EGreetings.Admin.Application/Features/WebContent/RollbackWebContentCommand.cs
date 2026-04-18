using EGreetings.Admin.Application.Common.Interfaces;
using EGreetings.Admin.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EGreetings.Admin.Application.Features.WebContent;

public record RollbackWebContentCommand(
    string Key,
    int VersionId,
    int AdminUserId) : IRequest<bool>;

public class RollbackWebContentCommandHandler : IRequestHandler<RollbackWebContentCommand, bool>
{
    private readonly IAdminDbContext _dbContext;
    private readonly ILogger<RollbackWebContentCommandHandler> _logger;

    public RollbackWebContentCommandHandler(IAdminDbContext dbContext, ILogger<RollbackWebContentCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> Handle(RollbackWebContentCommand request, CancellationToken cancellationToken)
    {
        var webContent = await _dbContext.WebContents
            .FirstOrDefaultAsync(w => w.Key == request.Key, cancellationToken);

        if (webContent == null)
        {
            _logger.LogWarning("WebContent with key {Key} not found", request.Key);
            return false;
        }

        var version = await _dbContext.WebContentVersions
            .FirstOrDefaultAsync(v => v.Id == request.VersionId && v.WebContentId == webContent.Id, cancellationToken);

        if (version == null)
        {
            _logger.LogWarning("Version {VersionId} not found for WebContent {Key}", request.VersionId, request.Key);
            return false;
        }

        // Archive current version
        var archivedVersion = new Domain.Entities.WebContentVersion
        {
            WebContentId = webContent.Id,
            Title = webContent.Title,
            Content = webContent.Content,
            ImageUrl = webContent.ImageUrl,
            Version = webContent.Version,
            CreatedByUserId = request.AdminUserId,
            ArchivedAt = DateTime.UtcNow
        };

        _dbContext.WebContentVersions.Add(archivedVersion);

        // Restore from archived version
        webContent.Title = version.Title;
        webContent.Content = version.Content;
        webContent.ImageUrl = version.ImageUrl;
        webContent.Version++;
        webContent.UpdatedByUserId = request.AdminUserId;

        // Create audit log
        var auditLog = new Domain.Entities.AuditLog
        {
            EventType = AuditEventType.WebContentRolledBack,
            EntityName = "WebContent",
            EntityId = webContent.Id,
            Description = $"Rolled back to version {version.Version}",
            UserId = request.AdminUserId,
            IpAddress = null,
            IsSystemAction = false,
            OccurredAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "WebContent rolled back: Key={Key}, VersionId={VersionId}, RolledBackBy={UserId}",
            request.Key, request.VersionId, request.AdminUserId);

        return true;
    }
}
