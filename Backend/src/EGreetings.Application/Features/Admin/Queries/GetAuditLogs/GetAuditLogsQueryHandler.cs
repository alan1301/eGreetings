using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Application.Features.Users.DTOs;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IRepository<AuditLog> _auditRepo;

    public GetAuditLogsQueryHandler(IRepository<AuditLog> auditRepo)
    {
        _auditRepo = auditRepo;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _auditRepo.Query();

        if (!string.IsNullOrEmpty(request.EntityName))
            query = query.Where(a => a.EntityName == request.EntityName);
        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);
        if (request.From.HasValue)
            query = query.Where(a => a.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(a => a.CreatedAt <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id, a.EventType.ToString(), a.EntityName, a.EntityId,
                a.Description, a.UserId, a.IpAddress, a.IsSystemAction, a.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, total, request.Page, request.PageSize);
    }
}
