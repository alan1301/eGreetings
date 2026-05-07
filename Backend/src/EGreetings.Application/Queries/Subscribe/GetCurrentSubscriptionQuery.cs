using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Subscribe.GetCurrentSubscription;

/// <summary>
/// UC19/UC25 – Lấy thông tin subscription hiện tại của user đã đăng nhập.
/// Trả về subscription Active (hoặc Expired) mới nhất, bao gồm plan type.
/// </summary>
public record GetCurrentSubscriptionQuery(Guid UserId) : IRequest<CurrentSubscriptionResult?>;

/// <summary>
/// Kết quả trả về cho frontend để hiển thị dashboard.
/// Plan = "monthly" | "annual" tuỳ theo PaymentMethod và thời hạn.
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
        // Lấy subscription mới nhất (Active ưu tiên, rồi đến Pending, Expired, Disabled)
        var subscription = await _db.Subscriptions
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s =>
                s.Status == SubscriptionStatus.Active ? 4 :
                s.Status == SubscriptionStatus.Pending ? 3 :
                s.Status == SubscriptionStatus.Expired ? 2 : 1)
            .ThenByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (subscription == null) return null;

        // Tính days remaining
        var daysRemaining = 0;
        if (subscription.ExpiryDate.HasValue)
        {
            var diff = subscription.ExpiryDate.Value - DateTime.UtcNow;
            daysRemaining = Math.Max(0, (int)diff.TotalDays);
        }

        // Xác định plan: annual nếu ExpiryDate - StartDate > 60 ngày, còn lại là monthly
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
