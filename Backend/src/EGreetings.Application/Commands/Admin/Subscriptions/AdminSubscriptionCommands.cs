using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Events;
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
                "Only subscriptions with Pending status can be activated.");

        // BR-15: Activate after payment confirmed
        var now = DateTime.UtcNow;
        sub.Status = SubscriptionStatus.Active;
        sub.StartDate = now;
        sub.ExpiryDate = now.AddDays(BusinessConstants.SubscriptionRenewalDays);
        sub.UpdatedAt = now;

        var planPrice = sub.Plan == SubscriptionPlan.Annual
            ? BusinessConstants.AnnualPlanPriceUsd
            : BusinessConstants.MonthlyPlanPriceUsd;

        await _db.PaymentTransactions.AddAsync(new PaymentTransaction
        {
            SubscriptionId = sub.Id,
            Amount         = planPrice,
            Currency       = "USD",
            PaymentMethod  = PaymentMethod.CardPayment,
            Status         = PaymentStatus.Paid,
            PaidAt         = now,
        }, ct);

        sub.RaiseDomainEvent(new SubscriptionActivatedEvent(sub.Id, sub.UserId, sub.Plan));
        await _db.SaveChangesAsync(ct);

        // Notify user — best-effort, don't fail if SMTP unavailable
        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                To = sub.User.Email,
                ToName = sub.User.FullName,
                Subject = "Your E-Greetings Subscription is Now Active",
                HtmlBody = $"""
                    <h2>Your Subscription is Now Active!</h2>
                    <p>Expiry date: <strong>{sub.ExpiryDate:dd/MM/yyyy}</strong></p>
                """,
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch { /* email failure is non-fatal */ }

        await _audit.LogAsync(EventType.AdminAction,
            $"[UC13] Admin activated Subscription #{sub.Id} for {sub.User.Email}",
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
        RuleFor(x => x.Reason).NotEmpty().WithMessage("A reason for disabling the subscription is required.");
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
                Subject = "Your E-Greetings Subscription Has Been Disabled",
                HtmlBody = $"""
                    <h2>Your Subscription Has Been Disabled</h2>
                    <p><strong>Reason:</strong> {request.Reason}</p>
                    <p>Please contact the administrator for more information.</p>
                """,
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch { /* email failure is non-fatal */ }

        await _audit.LogAsync(EventType.AdminAction,
            $"[UC14] Admin disabled Subscription #{sub.Id}: {request.Reason}",
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
    DateTime? ExpiryDate = null,     // null = no expiry (auto-renew)
    string? Notes = null
) : IRequest<Guid>;

public class AdminGrantSubscriptionCommandValidator : AbstractValidator<AdminGrantSubscriptionCommand>
{
    public AdminGrantSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        When(x => x.ExpiryDate.HasValue, () =>
        {
            RuleFor(x => x.ExpiryDate!.Value)
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.");
        });
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

        var expiryLabel = request.ExpiryDate.HasValue
            ? $"valid until **{request.ExpiryDate.Value:dd/MM/yyyy}**"
            : "with **no expiry limit** (auto-renew)";

        var subscription = new Subscription
        {
            UserId = request.UserId,
            Status = SubscriptionStatus.Active,
            Plan = request.Plan,
            PaymentMethod = PaymentMethod.AdminGrant,
            StartDate = now,
            ExpiryDate = request.ExpiryDate,
        };

        await _db.Subscriptions.AddAsync(subscription, ct);

        // Set gift notification so user sees a popup on next login
        user.PendingGiftMessage =
            $"🎉 Congratulations! You have been gifted a **{planLabel}** subscription {expiryLabel}." +
            (string.IsNullOrEmpty(request.Notes) ? "" : $" Note: {request.Notes}");

        await _db.SaveChangesAsync(ct);

        // Email is best-effort — don't fail the whole operation if SMTP is unavailable
        try
        {
            var expiryHtml = request.ExpiryDate.HasValue
                ? $"<p>Expiry date: <strong>{request.ExpiryDate.Value:dd/MM/yyyy}</strong></p>"
                : "<p>This subscription has <strong>no expiry</strong> and renews automatically.</p>";

            await _emailService.SendAsync(new EmailMessage
            {
                To = user.Email,
                ToName = user.FullName,
                Subject = $"You've Been Gifted a {planLabel} Subscription!",
                HtmlBody = $"""
                    <h2>🎉 Congratulations! You've received a {planLabel} subscription!</h2>
                    <p>An admin has granted you a <strong>{planLabel}</strong> subscription.</p>
                    {expiryHtml}
                    {(string.IsNullOrEmpty(request.Notes) ? "" : $"<p>Note: {request.Notes}</p>")}
                    <p>Log in now to explore!</p>
                """,
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch { /* email failure is non-fatal */ }

        var auditExpiry = request.ExpiryDate.HasValue
            ? $"expires {request.ExpiryDate.Value:dd/MM/yyyy}"
            : "no expiry (auto-renew)";

        await _audit.LogAsync(EventType.AdminAction,
            $"[Admin] Admin granted {planLabel} subscription to {user.Email}, {auditExpiry}" +
            (string.IsNullOrEmpty(request.Notes) ? "" : $" | Note: {request.Notes}"),
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return subscription.Id;
    }
}

// ────────────────────────────────────────────────────────────────────
// Admin: Reject a Pending subscription (bank transfer not confirmed)
// Pending → Disabled with rejection reason sent to user via email
// ────────────────────────────────────────────────────────────────────
public record RejectSubscriptionCommand(
    Guid SubscriptionId,
    Guid AdminId,
    string Reason
) : IRequest<Unit>;

public class RejectSubscriptionCommandValidator : AbstractValidator<RejectSubscriptionCommand>
{
    public RejectSubscriptionCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("A reason for rejecting the subscription is required.");
    }
}

public class RejectSubscriptionCommandHandler : IRequestHandler<RejectSubscriptionCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;

    public RejectSubscriptionCommandHandler(IAppDbContext db, IEmailService emailService, IAuditLogService audit)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
    }

    public async Task<Unit> Handle(RejectSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new EntityNotFoundException("Subscription", request.SubscriptionId);

        if (sub.Status != SubscriptionStatus.Pending)
            throw new BusinessRuleViolationException("BR-15",
                "Only subscriptions with Pending status can be rejected.");

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
                Subject = "Your E-Greetings Payment Request Has Been Rejected",
                HtmlBody = $"""
                    <h2>Payment Request Rejected</h2>
                    <p>We're sorry, but your subscription payment request has been rejected by our admin team.</p>
                    <p><strong>Reason:</strong> {request.Reason}</p>
                    <p>If you believe this is an error or need assistance, please contact us at <a href="mailto:support@e-greetings.com">support@e-greetings.com</a>.</p>
                """,
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch { /* email failure is non-fatal */ }

        await _audit.LogAsync(EventType.AdminAction,
            $"[Admin] Admin rejected Subscription #{sub.Id} for {sub.User.Email}: {request.Reason}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}

