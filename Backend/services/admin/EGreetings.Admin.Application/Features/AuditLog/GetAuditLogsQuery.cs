using EGreetings.Admin.Application.Common.Interfaces;
using EGreetings.Admin.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Admin.Application.Features.AuditLog;

public record GetAuditLogsQuery(
    string? EntityName = null,
    int? UserId = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<AuditLogDto>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IAdminDbContext _dbContext;

    public GetAuditLogsQueryHandler(IAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityName))
            query = query.Where(a => a.EntityName == request.EntityName);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId);

        if (request.From.HasValue)
            query = query.Where(a => a.OccurredAt >= request.From);

        if (request.To.HasValue)
            query = query.Where(a => a.OccurredAt <= request.To);

        var totalCount = await query.CountAsync(cancellationToken);

        var auditLogs = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.EventType,
                a.EntityName,
                a.EntityId,
                a.Description,
                a.UserId,
                a.IpAddress,
                a.IsSystemAction,
                a.OccurredAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(auditLogs, request.Page, request.PageSize, totalCount);
    }
}
