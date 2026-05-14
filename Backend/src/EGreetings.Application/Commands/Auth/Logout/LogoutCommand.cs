using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Auth.Logout;

/// <summary>UC18 – BR-25: Revoke JWT/session immediately.</summary>
public record LogoutCommand(Guid UserId) : IRequest<Unit>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public LogoutCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user != null)
        {
            // BR-25: Revoke refresh token immediately
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiry = null;
            await _db.SaveChangesAsync(ct);

            await _audit.LogAsync(EventType.Logout,
                $"[UC02] User logged out: {user.Email}",
                actorId: user.Id, actorType: ActorType.User,
                cancellationToken: ct);
        }

        return Unit.Value;
    }
}
