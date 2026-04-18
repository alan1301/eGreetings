using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;
using EGreetings.Subscription.Domain.Entities;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, CreateSubscriptionResponse>
{
    private readonly ISubscriptionDbContext _context;

    public RenewSubscriptionCommandHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<CreateSubscriptionResponse> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken);

        if (subscription == null)
        {
            throw new InvalidOperationException("Subscription not found");
        }

        // BR-30: Cannot renew if disabled by admin
        if (subscription.IsDisabledByAdmin)
        {
            throw new InvalidOperationException("Cannot renew a subscription that has been disabled by admin");
        }

        if (subscription.Plan == null)
        {
            throw new InvalidOperationException("Subscription plan not found");
        }

        // Create payment for renewal
        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            UserId = request.UserId,
            Amount = subscription.Plan.PricePerYear,
            Status = PaymentStatus.Pending,
            Method = request.PaymentMethod ?? "Unknown"
        };

        await _context.Payments.AddAsync(payment, cancellationToken);

        // Update subscription dates (pending confirmation)
        subscription.StartDate = DateTime.UtcNow;
        subscription.ExpiredAt = DateTime.UtcNow.AddYears(1);
        subscription.Status = SubscriptionStatus.Pending;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateSubscriptionResponse(
            subscription.Id,
            payment.Id,
            subscription.Plan.PricePerYear,
            "Renewal payment created. Pending confirmation."
        );
    }
}
