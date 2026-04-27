using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Subscribe.RenewSubscription;

/// <summary>
/// UC26 – Renew subscription.
/// BR-30: Add exactly 30 days from ExpiryDate (if still valid) or from today (if expired).
/// BR-15: Status Active only after payment confirmed.
/// Can only renew Active or Expired – NOT Disabled (must re-register via UC08).
/// </summary>
public record RenewSubscriptionCommand(
    Guid SubscriptionId,
    Guid UserId
) : IRequest<RenewSubscriptionResult>;

public record RenewSubscriptionResult(DateTime NewExpiryDate, string Message);

public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, RenewSubscriptionResult>
{
    private readonly IAppDbContext _db;

    public RenewSubscriptionCommandHandler(IAppDbContext db) => _db = db;

    public async Task<RenewSubscriptionResult> Handle(RenewSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, ct)
            ?? throw new EntityNotFoundException("Subscription", request.SubscriptionId);

        // Cannot renew Disabled subscription (must re-register)
        if (sub.Status == SubscriptionStatus.Disabled)
            throw new BusinessRuleViolationException("SUBSCRIBE",
                "Dịch vụ đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");

        if (sub.Status == SubscriptionStatus.Pending)
            throw new BusinessRuleViolationException("SUBSCRIBE",
                "Dịch vụ đang chờ xác nhận thanh toán.");

        // BR-30: Calculate new expiry
        var baseDate = (sub.ExpiryDate.HasValue && sub.ExpiryDate > DateTime.UtcNow)
            ? sub.ExpiryDate.Value
            : DateTime.UtcNow;

        var newExpiry = baseDate.AddDays(BusinessConstants.SubscriptionRenewalDays);

        // NOTE: In real flow, payment must be confirmed first (BR-15).
        // This handler assumes payment has already been verified (called from webhook handler).
        sub.Status = SubscriptionStatus.Active;
        sub.ExpiryDate = newExpiry;
        if (!sub.StartDate.HasValue) sub.StartDate = DateTime.UtcNow;
        sub.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new RenewSubscriptionResult(newExpiry, $"Gia hạn thành công đến {newExpiry:dd/MM/yyyy}.");
    }
}
