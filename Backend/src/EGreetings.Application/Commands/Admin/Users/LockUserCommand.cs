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
            activeSub.DisabledReason = $"Account locked by Admin: {request.Reason}";
            activeSub.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                To = user.Email,
                Subject = "Your Account Has Been Locked",
                HtmlBody = $"<p>Your account has been locked. Reason: {request.Reason}. Please contact the administrator.</p>",
                ReplyTo = "support@e-greetings.com"
            }, ct);
        }
        catch (Exception)
        {
            // Ignore email errors in development if SMTP is down
        }

        await _audit.LogAsync(EventType.AdminAction,
            $"[UC21] Admin locked account: {user.Email} | Reason: {request.Reason}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}
