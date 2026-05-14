using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Admin;

// ──── Admin: Payment Transactions History ────
public record PaymentTransactionAdminDto(
    Guid Id,
    Guid SubscriptionId,
    string UserEmail,
    string UserFullName,
    string Plan,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string? GatewayProvider,
    string? GatewayTransactionId,
    string Status,
    DateTime? PaidAt,
    DateTime CreatedAt);

public record GetPaymentTransactionsQuery(
    string? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20)
    : IRequest<PagedResult<PaymentTransactionAdminDto>>;

public class GetPaymentTransactionsQueryHandler
    : IRequestHandler<GetPaymentTransactionsQuery, PagedResult<PaymentTransactionAdminDto>>
{
    private readonly IAppDbContext _db;
    public GetPaymentTransactionsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<PaymentTransactionAdminDto>> Handle(
        GetPaymentTransactionsQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);
        var query = _db.PaymentTransactions
            .Include(p => p.Subscription).ThenInclude(s => s.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<PaymentStatus>(request.Status, true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p =>
                p.Subscription.User.Email.Contains(request.Search) ||
                p.Subscription.User.FullName.Contains(request.Search) ||
                (p.GatewayTransactionId != null && p.GatewayTransactionId.Contains(request.Search)));

        query = query.OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(p => new PaymentTransactionAdminDto(
                p.Id,
                p.SubscriptionId,
                p.Subscription.User.Email,
                p.Subscription.User.FullName,
                p.Subscription.Plan.ToString(),
                p.Amount,
                p.Currency,
                p.PaymentMethod.ToString(),
                p.GatewayProvider,
                p.GatewayTransactionId,
                p.Status.ToString(),
                p.PaidAt,
                p.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<PaymentTransactionAdminDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}
