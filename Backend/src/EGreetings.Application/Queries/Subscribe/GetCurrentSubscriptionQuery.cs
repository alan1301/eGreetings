using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Subscribe.GetCurrentSubscription;

/// <summary>
/// UC19/UC25 – Retrieves the current subscription info for the logged-in user.
/// Returns the most recent Active (or Expired) subscription, including plan type.
/// </summary>
public record GetCurrentSubscriptionQuery(Guid UserId) : IRequest<CurrentSubscriptionResult?>;

/// <summary>
/// Result returned to the frontend for dashboard display.
/// Plan = "monthly" | "annual" based on PaymentMethod and subscription duration.
/// </summary>
public record CurrentSubscriptionResult(
    Guid SubscriptionId,
    string Status,        // "Active" | "Expired" | "Pending" | "Disabled"
    string Plan,          // "monthly" | "annual"
    DateTime? StartDate,
    DateTime? ExpiryDate,
    int DaysRemaining,
    string PaymentMethod
);

public class GetCurrentSubscriptionQueryHandler
    : IRequestHandler<GetCurrentSubscriptionQuery, CurrentSubscriptionResult?>
{
    private readonly IAppDbContext _db;

    public GetCurrentSubscriptionQueryHandler(IAppDbContext db) => _db = db;

    public async Task<CurrentSubscriptionResult?> Handle(
        GetCurrentSubscriptionQuery request,
        CancellationToken ct)
    {
        // Get the most recent subscription (Active first, then Pending, Expired, Disabled)
        var subscription = await _db.Subscriptions
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s =>
                s.Status == SubscriptionStatus.Active ? 4 :
                s.Status == SubscriptionStatus.Pending ? 3 :
                s.Status == SubscriptionStatus.Expired ? 2 : 1)
            .ThenByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (subscription == null) return null;

        // Calculate days remaining
        var daysRemaining = 0;
        if (subscription.ExpiryDate.HasValue)
        {
            var diff = subscription.ExpiryDate.Value - DateTime.UtcNow;
            daysRemaining = Math.Max(0, (int)diff.TotalDays);
        }

        // Determine plan: annual if ExpiryDate - StartDate > 60 days, otherwise monthly
        var plan = "monthly";
        if (subscription.StartDate.HasValue && subscription.ExpiryDate.HasValue)
        {
            var duration = subscription.ExpiryDate.Value - subscription.StartDate.Value;
            if (duration.TotalDays > 60) plan = "annual";
        }

        return new CurrentSubscriptionResult(
            subscription.Id,
            subscription.Status.ToString(),
            plan,
            subscription.StartDate,
            subscription.ExpiryDate,
            daysRemaining,
            subscription.PaymentMethod.ToString()
        );
    }
}
