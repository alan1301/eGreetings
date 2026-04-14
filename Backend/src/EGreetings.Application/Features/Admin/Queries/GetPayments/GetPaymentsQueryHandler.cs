using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetPayments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .Include(p => p.Subscription)
            .AsQueryable();

        // Filter by status if provided
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<PaymentStatus>(request.Status, true, out var status))
                query = query.Where(p => p.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PaymentDto(
                p.Id,
                p.UserId,
                p.SubscriptionId,
                p.TransactionCode,
                p.Amount,
                p.Currency,
                p.Status.ToString(),
                p.PaymentMethod,
                p.PaidAt,
                p.Note,
                p.CreatedAt,
                p.Subscription != null ? p.Subscription.ExpiredAt : null
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentDto>(items, total, request.Page, request.PageSize);
    }
}
