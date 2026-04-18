using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Application.Features.Admin;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly ISubscriptionDbContext _context;

    public GetPaymentsQueryHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments.AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<PaymentStatus>(request.Status, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }
        }

        var total = await query.CountAsync(cancellationToken);

        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PaymentDto(
                p.Id,
                p.SubscriptionId,
                p.Amount,
                p.Status.ToString(),
                p.Method,
                p.TransactionCode,
                p.PaidAt,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentDto>(payments, total, request.Page, request.PageSize);
    }
}
