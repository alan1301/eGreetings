using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Subscribe.CreateSubscription;

/// <summary>
/// UC08 – Register subscribe service.
/// BR-14: Min 10 email. BR-15: Status = Pending until payment confirmed.
/// BR-22: Auto-create account for Guest after payment.
/// </summary>
public record CreateSubscriptionCommand(
    Guid UserId,
    List<string> EmailList,
    string PaymentMethod,     // "CardPayment"
    string Plan = "monthly"   // "monthly" | "annual"
) : IRequest<CreateSubscriptionResult>;

public record CreateSubscriptionResult(Guid SubscriptionId, string Status);

public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        // BR-14: At least 10 emails
        RuleFor(x => x.EmailList)
            .NotNull()
            .Must(list => list != null && list.Count >= BusinessConstants.MinSubscriptionEmailCount)
            .WithMessage($"At least {BusinessConstants.MinSubscriptionEmailCount} email addresses are required.");

        // Validate each email
        RuleForEach(x => x.EmailList)
            .EmailAddress().WithMessage("Invalid email address: '{PropertyValue}'.");

        RuleFor(x => x.PaymentMethod)
            .Must(m => m == "CardPayment")
            .WithMessage("Invalid payment method.");

        RuleFor(x => x.Plan)
            .Must(p => p == "monthly" || p == "annual")
            .WithMessage("Plan must be 'monthly' or 'annual'.");
    }
}

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, CreateSubscriptionResult>
{
    private readonly IAppDbContext _db;

    public CreateSubscriptionCommandHandler(IAppDbContext db) => _db = db;

    public async Task<CreateSubscriptionResult> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        var paymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, ignoreCase: true);
        var plan = request.Plan.Equals("annual", StringComparison.OrdinalIgnoreCase)
            ? SubscriptionPlan.Annual
            : SubscriptionPlan.Monthly;

        // BR-15: Status starts as Pending; admin will set StartDate/ExpiryDate on activation.
        var subscription = new Subscription
        {
            UserId = request.UserId,
            Status = SubscriptionStatus.Pending,
            Plan = plan,
            PaymentMethod = paymentMethod
        };

        await _db.Subscriptions.AddAsync(subscription, ct);
        await _db.SaveChangesAsync(ct);

        // BR-14: Save email list (validated >= 10)
        var uniqueEmails = request.EmailList
            .Select(e => e.ToLower().Trim())
            .Distinct()
            .ToList();

        foreach (var email in uniqueEmails)
        {
            await _db.SubscriptionEmailLists.AddAsync(new SubscriptionEmailList
            {
                SubscriptionId = subscription.Id,
                Email = email
            }, ct);
        }

        // UC08/UC13: Record the user-side payment as Paid; admin still confirms activation.
        var amount = plan == SubscriptionPlan.Annual
            ? BusinessConstants.AnnualPlanPriceUsd
            : BusinessConstants.MonthlyPlanPriceUsd;

        await _db.PaymentTransactions.AddAsync(new PaymentTransaction
        {
            SubscriptionId = subscription.Id,
            Amount = amount,
            Currency = "USD",
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow,
            GatewayProvider = "MockCardGateway",
            GatewayTransactionId = $"MOCK-{Guid.NewGuid():N}"
        }, ct);

        await _db.SaveChangesAsync(ct);

        return new CreateSubscriptionResult(subscription.Id, "AwaitingPayment");
    }
}
