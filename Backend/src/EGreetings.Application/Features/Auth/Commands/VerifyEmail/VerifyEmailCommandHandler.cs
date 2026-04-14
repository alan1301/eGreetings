using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public VerifyEmailCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token, cancellationToken)
            ?? throw new InvalidOperationException("Token không hợp lệ hoặc đã hết hạn.");

        user.EmailVerifiedAt = DateTime.UtcNow;
        user.Status = UserStatus.Active;
        user.EmailVerificationToken = null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.EmailVerified, "User", user.Id.ToString(),
            $"Email {user.Email} đã được xác thực.", cancellationToken: cancellationToken);

        return true;
    }
}
