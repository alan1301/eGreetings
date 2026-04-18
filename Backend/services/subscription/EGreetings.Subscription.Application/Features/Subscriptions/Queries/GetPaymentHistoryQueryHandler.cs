using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Queries;

public class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, List<PaymentDto>>
{
    private readonly ISubscriptionDbContext _context;

    public GetPaymentHistoryQueryHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentDto>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var payments = await _context.Payments
            .Where(p => p.UserId == request.UserId)
            .OrderByDescending(p => p.CreatedAt)
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

        return payments;
    }
}
