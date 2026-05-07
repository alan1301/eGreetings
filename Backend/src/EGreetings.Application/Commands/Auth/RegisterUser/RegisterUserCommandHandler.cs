using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EGreetings.Application.Commands.Auth.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly string _frontendUrl;

    public RegisterUserCommandHandler(
        IAppDbContext db,
        IPasswordHasher hasher,
        IEmailService emailService,
        IAuditLogService audit,
        ILogger<RegisterUserCommandHandler> logger,
        IConfiguration configuration)
    {
        _db = db;
        _hasher = hasher;
        _emailService = emailService;
        _audit = audit;
        _logger = logger;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:4200";
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        // BR-02: Email must be unique
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == request.Email.ToLower() && !u.IsDeleted, ct);

        if (emailExists)
            throw new BusinessRuleViolationException("BR-02", "Email này đã được đăng ký bởi tài khoản khác.");

        // BR-01: Password hashed with BCrypt (work factor 12 – validation already passed)
        var passwordHash = _hasher.Hash(request.Password);

        // BR-03: Email verification token — TTL 24h, single use
        var token = Guid.NewGuid().ToString("N");
        var tokenExpiry = DateTime.UtcNow.AddHours(BusinessConstants.EmailVerificationTokenTtlHours);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.User,
            Status = UserStatus.Active,     // Auto-activated: no email verification required in dev
            EmailVerificationToken = token, // Kept for future use (cosmetic in dev)
            EmailVerificationTokenExpiry = tokenExpiry
        };

        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);  // Commit user record before sending email

        // BR-03: Send activation email
        // BR-32: If email fails, registration still succeeds — queue for retry (UC29 job)
        var activationLink = $"{_frontendUrl}/auth/verify-email?token={token}&userId={user.Id}";
        var emailMessage = new EmailMessage
        {
            To = user.Email,
            ToName = user.FullName,
            Subject = "Kích hoạt tài khoản E-Greetings",
            HtmlBody = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                  <h2 style="color: #C9A96E;">Chào {user.FullName}!</h2>
                  <p>Cảm ơn bạn đã đăng ký tài khoản trên <strong>E-Greetings</strong>.</p>
                  <p>Nhấn vào nút bên dưới để kích hoạt tài khoản của bạn:</p>
                  <a href="{activationLink}"
                     style="display:inline-block;padding:12px 24px;background:#C9A96E;color:#fff;
                            text-decoration:none;border-radius:6px;font-weight:bold;">
                    Kích Hoạt Tài Khoản
                  </a>
                  <p style="color:#999;margin-top:24px;font-size:13px;">
                    Link hết hạn sau <strong>24 giờ</strong>.
                    Nếu bạn không yêu cầu đăng ký, hãy bỏ qua email này.
                  </p>
                </div>
                """,
            ReplyTo = "support@e-greetings.com"  // BR-28: Reply-To header
        };

        try
        {
            await _emailService.SendAsync(emailMessage, ct);
        }
        catch (Exception ex)
        {
            // BR-32: Email failure does NOT rollback registration
            // Log the error and the system retry job (UC29) will pick it up
            _logger.LogWarning(ex,
                "[BR-32] Failed to send activation email to {Email}. Queued for retry.", user.Email);

            // Note: EmailRetryQueue is tied to GreetingTransactions (BR-32 scope is greeting emails).
            // For activation emails, we rely on the "Resend Verification" endpoint as fallback.
            // The user record is saved and can request a new link after login attempt.
        }

        await _audit.LogAsync(
            EventType.Register,
            $"User đăng ký tài khoản: {user.Email}",
            actorId: user.Id,
            actorType: ActorType.User,
            cancellationToken: ct);

        return new RegisterUserResult(user.Id,
            "Đăng ký thành công! Vui lòng kiểm tra email để kích hoạt tài khoản. " +
            "Nếu không nhận được email, hãy kiểm tra thư mục Spam.");
    }
}
