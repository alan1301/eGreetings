using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Users;

/// <summary>UC21 – Admin: Lock user account.</summary>
public record LockUserCommand(Guid TargetUserId, Guid AdminId, string Reason) : IRequest<Unit>;

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;

    public LockUserCommandHandler(IAppDbContext db, IEmailService emailService, IAuditLogService audit)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
    }

    public async Task<Unit> Handle(LockUserCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.TargetUserId && !u.IsDeleted, ct)
            ?? throw new EntityNotFoundException("User", request.TargetUserId);

        user.Status = UserStatus.Locked;
        user.LockReason = request.Reason;
        // Revoke all sessions
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        // Cancel active subscription if any
        var activeSub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.Status == SubscriptionStatus.Active, ct);
        if (activeSub != null)
        {
            activeSub.Status = SubscriptionStatus.Disabled;
            activeSub.DisabledReason = $"Tài khoản bị khóa bởi Admin: {request.Reason}";
            activeSub.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                To = user.Email,
                Subject = "Tài khoản bị khóa",
                HtmlBody = $"<p>Tài khoản của bạn đã bị khóa. Lý do: {request.Reason}. Liên hệ quản trị viên.</p>",
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch (Exception)
        {
            // Ignore email errors in development if SMTP is down
        }

        await _audit.LogAsync(EventType.AdminAction,
            $"[UC21] Admin khóa tài khoản: {user.Email} | Lý do: {request.Reason}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}
