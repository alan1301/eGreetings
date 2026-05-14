using EGreetings.Application.Interfaces;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Auth.ResetPassword;

/// <summary>UC22 step 2 – validate token and set new password.</summary>
public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword
) : IRequest<Unit>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();

        // BR-01: Password complexity
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(BusinessConstants.PasswordMinLength)
            .Matches("[A-Z]").WithMessage("Must contain at least 1 uppercase letter.")
            .Matches("[a-z]").WithMessage("Must contain at least 1 lowercase letter.")
            .Matches("[0-9]").WithMessage("Must contain at least 1 digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain at least 1 special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public ResetPasswordCommandHandler(IAppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token && !u.IsDeleted, ct);

        if (user == null)
            throw new BusinessRuleViolationException("BR-26", "Invalid password reset link.");

        // BR-26: Token must not be expired
        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new BusinessRuleViolationException("BR-26", "Password reset link has expired.");

        // BR-26: Token must not be already used
        if (user.PasswordResetTokenUsed)
            throw new BusinessRuleViolationException("BR-26", "This link has already been used.");

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        user.PasswordResetTokenUsed = true;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        // BR-25: Revoke all refresh tokens (force re-login)
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = null;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
