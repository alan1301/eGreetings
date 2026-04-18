using EGreetings.Admin.Application.Common.Interfaces;
using EGreetings.Admin.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Admin.Application.Features.AuditLog;

public record CreateAuditLogCommand(
    AuditEventType EventType,
    string EntityName,
    int? EntityId,
    string Description,
    int? UserId,
    string? UserEmail,
    string? IpAddress,
    bool IsSystemAction = false) : IRequest<int>;

public class CreateAuditLogCommandHandler : IRequestHandler<CreateAuditLogCommand, int>
{
    private readonly IAdminDbContext _dbContext;
    private readonly ILogger<CreateAuditLogCommandHandler> _logger;

    public CreateAuditLogCommandHandler(IAdminDbContext dbContext, ILogger<CreateAuditLogCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        var auditLog = new Domain.Entities.AuditLog
        {
            EventType = request.EventType,
            EntityName = request.EntityName,
            EntityId = request.EntityId,
            Description = request.Description,
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            IpAddress = request.IpAddress,
            IsSystemAction = request.IsSystemAction,
            OccurredAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created: EventType={EventType}, EntityName={EntityName}, UserId={UserId}",
            request.EventType, request.EntityName, request.UserId);

        return auditLog.Id;
    }
}
