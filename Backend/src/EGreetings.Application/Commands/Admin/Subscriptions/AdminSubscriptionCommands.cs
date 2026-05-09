using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Subscriptions;

// ────────────────────────────────────────────────────────────────────
// UC13 – Admin: Activate subscription (manual bank transfer confirmed)
// BR-15: Status only changes to Active after confirmation
// ────────────────────────────────────────────────────────────────────
public record ActivateSubscriptionCommand(
    Guid SubscriptionId,
    Guid AdminId
) : IRequest<Unit>;

public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;

    public ActivateSubscriptionCommandHandler(IAppDbContext db, IEmailService emailService, IAuditLogService audit)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
    }

    public async Task<Unit> Handle(ActivateSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new EntityNotFoundException("Subscription", request.SubscriptionId);

        if (sub.Status != SubscriptionStatus.Pending)
            throw new BusinessRuleViolationException("BR-15",
                "Chỉ có thể kích hoạt đơn đang ở trạng thái Chờ xác nhận.");

        // BR-15: Activate after payment confirmed
        sub.Status = SubscriptionStatus.Active;
        sub.StartDate = DateTime.UtcNow;
        sub.ExpiryDate = DateTime.UtcNow.AddDays(BusinessConstants.SubscriptionRenewalDays);
        sub.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Notify user — best-effort, don't fail if SMTP unavailable
        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                To = sub.User.Email,
                ToName = sub.User.FullName,
                Subject = "Dịch vụ Subscribe đã được kích hoạt",
                HtmlBody = $"""
                    <h2>Dịch vụ Subscribe đã kích hoạt!</h2>
                    <p>Ngày hết hạn: <strong>{sub.ExpiryDate:dd/MM/yyyy}</strong></p>
                """,
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch { /* email failure is non-fatal */ }

        await _audit.LogAsync(EventType.AdminAction,
            $"[UC13] Admin kích hoạt Subscribe #{sub.Id} cho {sub.User.Email}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}

// ────────────────────────────────────────────────────────────────────
// UC14 – Admin: Disable subscription
// ────────────────────────────────────────────────────────────────────
public record DisableSubscriptionCommand(
    Guid SubscriptionId,
    Guid AdminId,
    string Reason
) : IRequest<Unit>;

public class DisableSubscriptionCommandValidator : AbstractValidator<DisableSubscriptionCommand>
{
    public DisableSubscriptionCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Phải nhập lý do vô hiệu hóa");
    }
}

public class DisableSubscriptionCommandHandler : IRequestHandler<DisableSubscriptionCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;

    public DisableSubscriptionCommandHandler(IAppDbContext db, IEmailService emailService, IAuditLogService audit)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
    }

    public async Task<Unit> Handle(DisableSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new EntityNotFoundException("Subscription", request.SubscriptionId);

        // State machine: Active → Disabled only
        sub.Status = SubscriptionStatus.Disabled;
        sub.DisabledReason = request.Reason;
        sub.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Notify user — best-effort, don't fail if SMTP unavailable
        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                To = sub.User.Email,
                ToName = sub.User.FullName,
                Subject = "Dịch vụ Subscribe bị vô hiệu hóa",
                HtmlBody = $"""
                    <h2>Dịch vụ Subscribe bị vô hiệu hóa</h2>
                    <p><strong>Lý do:</strong> {request.Reason}</p>
                    <p>Vui lòng liên hệ quản trị viên để biết thêm chi tiết.</p>
                """,
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch { /* email failure is non-fatal */ }

        await _audit.LogAsync(EventType.AdminAction,
            $"[UC14] Admin vô hiệu hóa Subscribe #{sub.Id}: {request.Reason}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}

// ────────────────────────────────────────────────────────────────────
// Admin: Grant subscription directly to a user (immediately Active)
// Admin override: bypasses BR-14 (min 10 email) — at least 1 required
// ────────────────────────────────────────────────────────────────────
public record AdminGrantSubscriptionCommand(
    Guid UserId,
    Guid AdminId,
    SubscriptionPlan Plan,
    DateTime ExpiryDate,
    string? Notes = null
) : IRequest<Guid>;

public class AdminGrantSubscriptionCommandValidator : AbstractValidator<AdminGrantSubscriptionCommand>
{
    public AdminGrantSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Ngày kết thúc phải sau ngày hiện tại");
    }
}

public class AdminGrantSubscriptionCommandHandler : IRequestHandler<AdminGrantSubscriptionCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;

    public AdminGrantSubscriptionCommandHandler(IAppDbContext db, IEmailService emailService, IAuditLogService audit)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
    }

    public async Task<Guid> Handle(AdminGrantSubscriptionCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, ct)
            ?? throw new EntityNotFoundException("User", request.UserId);

        var now = DateTime.UtcNow;
        var planLabel = request.Plan switch
        {
            SubscriptionPlan.Free    => "Free",
            SubscriptionPlan.Monthly => "Monthly",
            SubscriptionPlan.Annual  => "Annual",
            _                        => "Subscribe"
        };

        var daysLeft = (int)Math.Ceiling((request.ExpiryDate - now).TotalDays);

        var subscription = new Subscription
        {
            UserId = request.UserId,
            Status = SubscriptionStatus.Active,
            Plan = request.Plan,
            PaymentMethod = PaymentMethod.BankTransfer,
            StartDate = now,
            ExpiryDate = request.ExpiryDate,
        };

        await _db.Subscriptions.AddAsync(subscription, ct);

        // Set gift notification so user sees a popup on next login
        user.PendingGiftMessage =
            $"🎉 Chúc mừng! Bạn đã được tặng gói **{planLabel}** miễn phí trong **{daysLeft} ngày**, " +
            $"có hiệu lực đến **{request.ExpiryDate:dd/MM/yyyy}**." +
            (string.IsNullOrEmpty(request.Notes) ? "" : $" Ghi chú: {request.Notes}");

        await _db.SaveChangesAsync(ct);

        // Email is best-effort — don't fail the whole operation if SMTP is unavailable
        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                To = user.Email,
                ToName = user.FullName,
                Subject = $"Bạn đã được tặng gói {planLabel} Subscribe!",
                HtmlBody = $"""
                    <h2>🎉 Chúc mừng! Bạn đã được tặng gói {planLabel}!</h2>
                    <p>Admin đã cấp gói <strong>{planLabel}</strong> cho tài khoản của bạn.</p>
                    <p>Ngày hết hạn: <strong>{request.ExpiryDate:dd/MM/yyyy}</strong></p>
                    {(string.IsNullOrEmpty(request.Notes) ? "" : $"<p>Ghi chú: {request.Notes}</p>")}
                    <p>Đăng nhập ngay để khám phá!</p>
                """,
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch { /* email failure is non-fatal */ }

        await _audit.LogAsync(EventType.AdminAction,
            $"[Admin] Admin cấp Subscribe {planLabel} cho {user.Email}, hết hạn {request.ExpiryDate:dd/MM/yyyy}" +
            (string.IsNullOrEmpty(request.Notes) ? "" : $" | Ghi chú: {request.Notes}"),
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return subscription.Id;
    }
}
