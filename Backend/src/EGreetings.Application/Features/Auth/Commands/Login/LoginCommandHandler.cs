using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Auth.DTOs;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;

    public LoginCommandHandler(IApplicationDbContext context, ITokenService tokenService,
        IAuditService auditService)
    {
        _context = context;
        _tokenService = tokenService;
        _auditService = auditService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken)
            ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        // BR-04: Check login lockout
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            var remainingMinutes = (int)Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
            throw new UnauthorizedAccessException(
                $"Tài khoản bị khóa tạm thời {remainingMinutes} phút. Vui lòng thử lại sau.");
        }

        if (user.Status == UserStatus.Banned)
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // BR-04: Increment failed attempts
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        }

        if (user.Status == UserStatus.Inactive)
            throw new UnauthorizedAccessException("Vui lòng xác thực email trước khi đăng nhập.");

        // Reset failed attempts on successful login
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.UserLoggedIn, "User", user.Id.ToString(),
            $"Người dùng {user.Email} đăng nhập thành công.", cancellationToken: cancellationToken);

        var token = _tokenService.GenerateJwtToken(user);

        return new AuthResponseDto(
            user.Id, user.FullName, user.Email,
            user.Role.ToString(), token, DateTime.UtcNow.AddHours(24));
    }
}
