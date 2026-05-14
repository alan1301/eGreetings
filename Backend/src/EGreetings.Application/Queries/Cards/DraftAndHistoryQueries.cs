using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Cards;

// ──────────── Drafts (UC17) ────────────
public record DraftDto(
    Guid Id, Guid CardId, string CardName, string? ThumbnailUrl,
    string? PersonalMessage, DateTime LastAutoSavedAt);

public record GetDraftsQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<DraftDto>>;

public class GetDraftsQueryHandler : IRequestHandler<GetDraftsQuery, PagedResult<DraftDto>>
{
    private readonly IAppDbContext _db;

    public GetDraftsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<DraftDto>> Handle(GetDraftsQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);
        var query = _db.Drafts
            .Include(d => d.Card)
            .Where(d => d.UserId == request.UserId && !d.IsDeleted)
            .OrderByDescending(d => d.LastAutoSavedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(d => new DraftDto(d.Id, d.CardId, d.Card.Name, d.Card.ThumbnailUrl,
                d.PersonalMessage, d.LastAutoSavedAt))
            .ToListAsync(ct);

        return new PagedResult<DraftDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}

// ──────────── Greeting History (UC17) ────────────
public record GreetingHistoryDto(
    Guid Id, string CardName, string? ThumbnailUrl,
    string RecipientEmail, DateTime? SentAt, string Status,
    bool IsFresh = false);

public record GetGreetingHistoryQuery(
    Guid UserId, string? Status = null,
    int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<GreetingHistoryDto>>;

public class GetGreetingHistoryQueryHandler : IRequestHandler<GetGreetingHistoryQuery, PagedResult<GreetingHistoryDto>>
{
    private readonly IAppDbContext _db;

    public GetGreetingHistoryQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<GreetingHistoryDto>> Handle(GetGreetingHistoryQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);
        var query = _db.GreetingTransactions
            .Include(t => t.Card)
            .Where(t => t.SenderId == request.UserId);

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<TransactionStatus>(request.Status, out var status))
            query = query.Where(t => t.Status == status);

        query = query.OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(t => new GreetingHistoryDto(
                t.Id,
                t.IsFresh ? (string.IsNullOrEmpty(t.Subject) ? "Personalized Card" : t.Subject) : t.Card.Name,
                t.IsFresh ? null : t.Card.ThumbnailUrl,
                t.RecipientEmail, t.SentAt, t.Status.ToString(),
                t.IsFresh))
            .ToListAsync(ct);

        return new PagedResult<GreetingHistoryDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}
