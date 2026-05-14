using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Users;

public record AdminUpdateUserCommand(
    Guid UserId,
    Guid AdminId,
    string FullName,
    string Email,
    string Role) : IRequest<Unit>;

public class AdminUpdateUserCommandHandler : IRequestHandler<AdminUpdateUserCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public AdminUpdateUserCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(AdminUpdateUserCommand request, CancellationToken ct)
    {
        if (request.UserId == request.AdminId)
            throw new BusinessRuleViolationException("ADMIN-01", "You cannot edit your own account from this panel.");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, ct)
            ?? throw new EntityNotFoundException("User", request.UserId);

        var emailTaken = await _db.Users
            .AnyAsync(u => u.Email == request.Email.ToLower().Trim() && u.Id != request.UserId && !u.IsDeleted, ct);
        if (emailTaken)
            throw new BusinessRuleViolationException("BR-02", "This email is already used by another account.");

        var role = Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var parsedRole)
            ? parsedRole
            : UserRole.User;

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.ToLower().Trim();
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction,
            $"[Admin] Updated user: {user.Email} | Role: {role}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}
