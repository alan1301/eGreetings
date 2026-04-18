using EGreetings.Identity.Application.Common.Interfaces;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IIdentityDbContext _dbContext;

    public ResetPasswordCommandHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.PasswordResetToken == request.Token);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid password reset token.");
        }

        // BR-06: Check expiry (1h)
        if (user.PasswordResetTokenExpiry.HasValue && user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Password reset token has expired.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match.");
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
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
