using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.Domain.Enums;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IIdentityDbContext _dbContext;

    public VerifyEmailCommandHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.EmailVerificationToken == request.Token);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid verification token.");
        }

        // BR-07: Check expiry (24h)
        if (user.EmailVerificationTokenExpiry.HasValue && user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Verification token has expired.");
        }

        user.IsEmailVerified = true;
        user.Status = UserStatus.Active;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
