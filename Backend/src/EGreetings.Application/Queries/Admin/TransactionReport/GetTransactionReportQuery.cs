using EGreetings.Application.Interfaces;
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
