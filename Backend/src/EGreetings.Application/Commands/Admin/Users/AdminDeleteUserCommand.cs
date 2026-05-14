using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Users;

public record AdminDeleteUserCommand(Guid UserId, Guid AdminId) : IRequest<Unit>;

public class AdminDeleteUserCommandHandler : IRequestHandler<AdminDeleteUserCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public AdminDeleteUserCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(AdminDeleteUserCommand request, CancellationToken ct)
    {
        if (request.UserId == request.AdminId)
            throw new BusinessRuleViolationException("ADMIN-02", "You cannot delete your own account.");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, ct)
            ?? throw new EntityNotFoundException("User", request.UserId);

        if (user.Role == UserRole.Admin)
            throw new BusinessRuleViolationException("ADMIN-03", "Admin accounts cannot be deleted.");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction,
            $"[Admin] Deleted user account: {user.Email}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}
