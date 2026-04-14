using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Subscriptions.Queries.GetPaymentHistory;

public class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, IReadOnlyList<PaymentDto>>
{
    private readonly IRepository<Payment> _paymentRepo;

    public GetPaymentHistoryQueryHandler(IRepository<Payment> paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        // UC25: Lịch sử thanh toán bao gồm "Ngày hết hạn Subscribe"
        var payments = await _paymentRepo.Query()
            .Include(p => p.Subscription)
            .Where(p => p.UserId == request.UserId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentDto(
                p.Id, p.UserId, p.SubscriptionId,
                p.TransactionCode, p.Amount, p.Currency,
                p.Status.ToString(), p.PaymentMethod,
                p.PaidAt, p.Note, p.CreatedAt,
                p.Subscription != null ? p.Subscription.ExpiredAt : null))
            .ToListAsync(cancellationToken);

        return payments;
    }
}
