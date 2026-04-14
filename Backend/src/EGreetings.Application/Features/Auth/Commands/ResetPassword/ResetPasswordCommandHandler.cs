using EGreetings.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ResetPasswordCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, cancellationToken)
            ?? throw new InvalidOperationException("Token không hợp lệ hoặc đã hết hạn.");

        if (user.PasswordResetExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Token đã hết hạn. Vui lòng yêu cầu lại.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
