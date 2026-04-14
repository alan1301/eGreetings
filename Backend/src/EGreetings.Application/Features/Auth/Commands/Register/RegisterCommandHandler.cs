using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Auth.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;

    public RegisterCommandHandler(IApplicationDbContext context, ITokenService tokenService,
        IEmailService emailService, IAuditService auditService)
    {
        _context = context;
        _tokenService = tokenService;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra email đã tồn tại
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException("Email đã được sử dụng.");

        var verificationToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = UserRole.User,
            Status = UserStatus.Inactive,  // Cần xác thực email
            EmailVerificationToken = verificationToken
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Gửi email xác thực
        await _emailService.SendEmailVerificationAsync(user.Email, verificationToken, cancellationToken);

        await _auditService.LogAsync(AuditEventType.UserRegistered, "User", user.Id.ToString(),
            $"Người dùng {user.Email} đã đăng ký.", cancellationToken: cancellationToken);

        var token = _tokenService.GenerateJwtToken(user);

        return new AuthResponseDto(
            user.Id, user.FullName, user.Email,
            user.Role.ToString(), token, DateTime.UtcNow.AddHours(24));
    }
}
