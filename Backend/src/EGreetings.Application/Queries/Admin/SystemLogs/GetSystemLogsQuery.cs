using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Admin;

// ──── Admin: System Logs (UC30) ────
public record SystemLogDto(
    Guid Id, DateTime Timestamp, string? ActorId, string ActorType,
    string EventType, string Description, string Status, string? IpAddress);

public record GetSystemLogsQuery(
    string? EventType = null,
    string? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1, int PageSize = 50)  // UC30: 50 per page
    : IRequest<PagedResult<SystemLogDto>>;

public class GetSystemLogsQueryHandler : IRequestHandler<GetSystemLogsQuery, PagedResult<SystemLogDto>>
{
    private readonly IAppDbContext _db;

    public GetSystemLogsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<SystemLogDto>> Handle(GetSystemLogsQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, 200);
        var query = _db.SystemLogs.AsQueryable();

        if (!string.IsNullOrEmpty(request.EventType) && Enum.TryParse<EventType>(request.EventType, out var et))
            query = query.Where(l => l.EventType == et);
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LogStatus>(request.Status, out var ls))
            query = query.Where(l => l.Status == ls);
        if (request.From.HasValue) query = query.Where(l => l.Timestamp >= request.From.Value);
        if (request.To.HasValue) query = query.Where(l => l.Timestamp <= request.To.Value);

        query = query.OrderByDescending(l => l.Timestamp);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(l => new SystemLogDto(
                l.Id, l.Timestamp, l.ActorId.HasValue ? l.ActorId.ToString() : null,
                l.ActorType.ToString(), l.EventType.ToString(), l.Description,
                l.Status.ToString(), l.IpAddress))
            .ToListAsync(ct);

        return new PagedResult<SystemLogDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}
