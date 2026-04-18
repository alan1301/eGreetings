using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;
using EGreetings.Subscription.Domain.Entities;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, CreateSubscriptionResponse>
{
    private readonly ISubscriptionDbContext _context;

    public CreateSubscriptionCommandHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<CreateSubscriptionResponse> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // BR-12: Check only 1 active subscription
        var existingActiveSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active, cancellationToken);

        if (existingActiveSubscription != null)
        {
            throw new InvalidOperationException("User already has an active subscription");
        }

        // Find plan
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

        if (plan == null)
        {
            throw new InvalidOperationException("Subscription plan not found");
        }

        // Create subscription
        var subscription = new global::EGreetings.Subscription.Domain.Entities.Subscription
        {
            UserId = request.UserId,
            PlanId = request.PlanId,
            Status = SubscriptionStatus.Pending,
            StartDate = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddYears(1),
            MaxRecipients = plan.MaxRecipients,
            IsDisabledByAdmin = false
        };

        await _context.Subscriptions.AddAsync(subscription, cancellationToken);

        // Create payment
        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            UserId = request.UserId,
            Amount = plan.PricePerYear,
            Status = PaymentStatus.Pending,
            Method = request.PaymentMethod
        };

        await _context.Payments.AddAsync(payment, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateSubscriptionResponse(
            subscription.Id,
            payment.Id,
            plan.PricePerYear,
            "Subscription created successfully. Pending payment confirmation."
        );
    }
}
