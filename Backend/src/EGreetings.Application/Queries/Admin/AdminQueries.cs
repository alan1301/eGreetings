using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Admin;

// ──── Admin: Transaction Report (UC12) ────
public record TransactionDto(
    Guid Id, string SenderEmail, string CardName, string RecipientEmail,
    DateTime CreatedAt, string Status, bool IsSubscribeSend);

public record GetTransactionReportQuery(
    DateTime? From = null, DateTime? To = null,
    string? Search = null,
    int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<TransactionDto>>;

public class GetTransactionReportQueryHandler : IRequestHandler<GetTransactionReportQuery, PagedResult<TransactionDto>>
{
    private readonly IAppDbContext _db;

    public GetTransactionReportQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<TransactionDto>> Handle(GetTransactionReportQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);
        var query = _db.GreetingTransactions
            .Include(t => t.Sender)
            .Include(t => t.Card)
            .AsQueryable();

        if (request.From.HasValue) query = query.Where(t => t.CreatedAt >= request.From.Value);
        if (request.To.HasValue) query = query.Where(t => t.CreatedAt <= request.To.Value);
        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(t =>
                t.Sender.Email.Contains(request.Search) ||
                t.RecipientEmail.Contains(request.Search) ||
                t.Card.Name.Contains(request.Search));

        query = query.OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(t => new TransactionDto(
                t.Id, t.Sender.Email, t.Card.Name, t.RecipientEmail,
                t.CreatedAt, t.Status.ToString(), t.SubscriptionId != null))
            .ToListAsync(ct);

        return new PagedResult<TransactionDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}

// ──── Admin: Feedback List (UC11) ────
public record FeedbackDto(
    Guid Id, string UserEmail, string UserFullName, string Title, string Content,
    int? StarRating, string Status, DateTime CreatedAt);

public record GetFeedbacksQuery(
    string? Status = null,
    int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<FeedbackDto>>;

public class GetFeedbacksQueryHandler : IRequestHandler<GetFeedbacksQuery, PagedResult<FeedbackDto>>
{
    private readonly IAppDbContext _db;

    public GetFeedbacksQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<FeedbackDto>> Handle(GetFeedbacksQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);
        var query = _db.Feedbacks
            .Include(f => f.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<FeedbackStatus>(request.Status, out var status))
            query = query.Where(f => f.Status == status);

        query = query.OrderByDescending(f => f.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(f => new FeedbackDto(f.Id, f.User.Email, f.User.FullName, f.Title, f.Content,
                f.StarRating, f.Status.ToString(), f.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<FeedbackDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}

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

// ──── Admin: Users List (UC21) ────
public record AdminUserDto(
    Guid Id, string FullName, string Email, string Role, string Status,
    string? SubscriptionStatus, string? SubscriptionPlan, DateTime? SubscriptionExpiry, Guid? SubscriptionId,
    int TotalCardsSent, DateTime CreatedAt);

public record GetAdminUsersQuery(
    string? Search = null, string? SubscriptionStatus = null,
    int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AdminUserDto>>;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, PagedResult<AdminUserDto>>
{
    private readonly IAppDbContext _db;

    public GetAdminUsersQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);

        var projected = _db.Users
            .Where(u => u.Role == UserRole.User && !u.IsDeleted)
            .Select(u => new
            {
                u.Id, u.FullName, u.Email, u.Role, u.Status, u.CreatedAt,
                LatestSub = u.Subscriptions.OrderByDescending(s => s.CreatedAt)
                    .Select(s => new { s.Id, s.Status, s.Plan, s.ExpiryDate })
                    .FirstOrDefault(),
                TotalSent = u.SentTransactions.Count()
            });

        if (!string.IsNullOrEmpty(request.Search))
            projected = projected.Where(x => x.FullName.Contains(request.Search) || x.Email.Contains(request.Search));

        if (!string.IsNullOrEmpty(request.SubscriptionStatus) &&
            Enum.TryParse<SubscriptionStatus>(request.SubscriptionStatus, out var ssFilter))
            projected = projected.Where(x => x.LatestSub != null && x.LatestSub.Status == ssFilter);

        projected = projected.OrderByDescending(x => x.CreatedAt);

        var total = await projected.CountAsync(ct);
        var items = await projected
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(x => new AdminUserDto(
                x.Id, x.FullName, x.Email, x.Role.ToString(), x.Status.ToString(),
                x.LatestSub != null ? x.LatestSub.Status.ToString() : null,
                x.LatestSub != null ? x.LatestSub.Plan.ToString() : null,
                x.LatestSub != null ? x.LatestSub.ExpiryDate : null,
                x.LatestSub != null ? x.LatestSub.Id : null,
                x.TotalSent, x.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<AdminUserDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}
