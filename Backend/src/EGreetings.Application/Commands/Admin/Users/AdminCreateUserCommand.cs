using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Users;

public record AdminCreateUserCommand(
    Guid AdminId,
    string FullName,
    string Email,
    string Password,
    string Role) : IRequest<Guid>;

public class AdminCreateUserCommandHandler : IRequestHandler<AdminCreateUserCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditLogService _audit;

    public AdminCreateUserCommandHandler(IAppDbContext db, IPasswordHasher hasher, IAuditLogService audit)
    {
        _db = db;
        _hasher = hasher;
        _audit = audit;
    }

    public async Task<Guid> Handle(AdminCreateUserCommand request, CancellationToken ct)
    {
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == request.Email.ToLower().Trim() && !u.IsDeleted, ct);
        if (emailExists)
            throw new BusinessRuleViolationException("BR-02", "This email is already registered.");

        var role = Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var parsedRole)
            ? parsedRole
            : UserRole.User;

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            Role = role,
            Status = UserStatus.Active
        };

        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction,
            $"[Admin] Created user account: {user.Email} | Role: {role}",
            actorId: request.AdminId, actorType: ActorType.Admin, cancellationToken: ct);

        return user.Id;
    }
}
