using EGreetings.Application.Interfaces;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Admin;

public record AdminSubscriptionDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string PaymentMethod,
    string Status,
    DateTime CreatedAt,
    DateTime? StartDate,
    DateTime? ExpiryDate,
    string? DisabledReason
);

public record GetAdminSubscriptionsQuery(
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AdminSubscriptionDto>>;

public class GetAdminSubscriptionsQueryHandler : IRequestHandler<GetAdminSubscriptionsQuery, PagedResult<AdminSubscriptionDto>>
{
    private readonly IAppDbContext _db;

    public GetAdminSubscriptionsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminSubscriptionDto>> Handle(GetAdminSubscriptionsQuery request, CancellationToken ct)
    {
        var query = _db.Subscriptions
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(s => s.Status.ToString() == request.Status);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new AdminSubscriptionDto(
                s.Id,
                s.UserId,
                s.User.Email,
                s.User.FullName,
                s.PaymentMethod.ToString(),
                s.Status.ToString(),
                s.CreatedAt,
                s.StartDate,
                s.ExpiryDate,
                s.DisabledReason
            ))
            .ToListAsync(ct);

        return new PagedResult<AdminSubscriptionDto>
        {
            Items = items,
            Meta = new PaginationMeta
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = totalCount
            }
        };
    }
}
