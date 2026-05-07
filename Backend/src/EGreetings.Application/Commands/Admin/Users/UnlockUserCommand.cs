using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Users;

public record UnlockUserCommand(Guid UserId) : IRequest<bool>;

public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, bool>
{
    private readonly IAppDbContext _db;

    public UnlockUserCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.Status = UserStatus.Active;
        user.LockReason = null;
        user.FailedLoginCount = 0;
        user.LockoutEndTime = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
