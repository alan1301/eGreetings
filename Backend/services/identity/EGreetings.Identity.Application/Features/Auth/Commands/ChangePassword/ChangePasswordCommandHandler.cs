using EGreetings.Identity.Application.Common.Interfaces;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IIdentityDbContext _dbContext;

    public ChangePasswordCommandHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Id == request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var currentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!currentPasswordValid)
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new InvalidOperationException("New passwords do not match.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters long.");
        }

        if (!request.NewPassword.Any(char.IsUpper))
        {
            throw new InvalidOperationException("Password must contain at least one uppercase letter.");
        }

        if (!request.NewPassword.Any(char.IsDigit))
        {
            throw new InvalidOperationException("Password must contain at least one digit.");
        }

        if (!request.NewPassword.Any(c => !char.IsLetterOrDigit(c)))
        {
            throw new InvalidOperationException("Password must contain at least one special character.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
