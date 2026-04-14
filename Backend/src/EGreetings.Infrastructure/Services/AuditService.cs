using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EGreetings.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IApplicationDbContext context, ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(AuditEventType eventType, string entityName, string? entityId,
        string description, string? oldValue = null, string? newValue = null,
        bool isSystemAction = false, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = isSystemAction ? null : _currentUser.UserId,
            EventType = eventType,
            EntityName = entityName,
            EntityId = entityId,
            Description = description,
            OldValue = oldValue,
            NewValue = newValue,
            IsSystemAction = isSystemAction,
            IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString()
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
