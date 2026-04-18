using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Application.Features.Admin;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, bool>
{
    private readonly ISubscriptionDbContext _context;

    public ConfirmPaymentCommandHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.UtcNow;
        payment.ConfirmedByAdminId = request.AdminUserId;
        payment.TransactionCode = request.TransactionCode;

        if (payment.Subscription != null)
        {
            payment.Subscription.Status = SubscriptionStatus.Active;
            payment.Subscription.StartDate = DateTime.UtcNow;
            payment.Subscription.ExpiredAt = DateTime.UtcNow.AddYears(1);
            _context.Subscriptions.Update(payment.Subscription);
        }

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Publish PaymentConfirmedEvent for messaging
        return true;
    }
}
