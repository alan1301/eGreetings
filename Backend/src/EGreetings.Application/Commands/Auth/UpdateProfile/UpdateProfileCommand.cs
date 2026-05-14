using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Auth.UpdateProfile;

// ─── UC19: Update Profile ───────────────────────────────────────────
public record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string? CurrentPassword,
    string? NewPassword,
    string? ConfirmNewPassword
) : IRequest<Unit>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);

        // Only validate new password fields when user wants to change password
        When(x => !string.IsNullOrEmpty(x.NewPassword), () =>
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            // BR-01: Password complexity
            RuleFor(x => x.NewPassword!)
                .MinimumLength(BusinessConstants.PasswordMinLength)
                .Matches("[A-Z]").WithMessage("New password must contain at least 1 uppercase letter.")
                .Matches("[a-z]").WithMessage("New password must contain at least 1 lowercase letter.")
                .Matches("[0-9]").WithMessage("New password must contain at least 1 digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least 1 special character.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        });
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditLogService _audit;

    public UpdateProfileCommandHandler(IAppDbContext db, IPasswordHasher hasher, IAuditLogService audit)
    {
        _db = db;
        _hasher = hasher;
        _audit = audit;
    }

    public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new EntityNotFoundException("User", request.UserId);

        user.FullName = request.FullName.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        // Change password if requested
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (!_hasher.Verify(request.CurrentPassword!, user.PasswordHash ?? string.Empty))
                throw new BusinessRuleViolationException("AUTH", "Current password is incorrect.");

            user.PasswordHash = _hasher.Hash(request.NewPassword);
            // BR-25: Revoke all refresh tokens on password change
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiry = null;
        }

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction,
            $"[UC19] User updated profile: {user.Email}",
            actorId: user.Id, actorType: ActorType.User, cancellationToken: ct);

        return Unit.Value;
    }
}

// ─── UC01 Step 2: Verify Email ──────────────────────────────────────
public record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Unit>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public VerifyEmailCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new EntityNotFoundException("User", request.UserId);

        // BR-03: Token must not be expired
        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            throw new BusinessRuleViolationException("BR-03",
                "Verification link has expired. Please request a new one.");

        if (user.EmailVerificationToken != request.Token)
            throw new BusinessRuleViolationException("BR-03",
                "Invalid verification link.");

        user.Status = UserStatus.Active;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.Register,
            $"[UC01] Email verified successfully: {user.Email}",
            actorId: user.Id, actorType: ActorType.User, cancellationToken: ct);

        return Unit.Value;
    }
}
