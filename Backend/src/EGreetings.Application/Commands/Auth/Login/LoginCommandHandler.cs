using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;
    private readonly IAuditLogService _audit;

    public LoginCommandHandler(
        IAppDbContext db,
        IPasswordHasher hasher,
        ITokenService tokenService,
        IAuditLogService audit)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
        _audit = audit;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower() && !u.IsDeleted, ct);

        if (user == null)
            throw new BusinessRuleViolationException("AUTH", "Email hoặc mật khẩu không đúng.");

        // BR-04: Check lockout
        if (user.LockoutEndTime.HasValue && user.LockoutEndTime > DateTime.UtcNow)
            throw new BusinessRuleViolationException("BR-04",
                $"Tài khoản bị khóa đến {user.LockoutEndTime:HH:mm}. Vui lòng thử lại sau.");

        // BR-03: Must be activated
        if (user.Status == UserStatus.PendingActivation)
            throw new BusinessRuleViolationException("BR-03", "Tài khoản chưa được xác thực email.");

        // BR-04: Must be active
        if (user.Status == UserStatus.Disabled)
            throw new BusinessRuleViolationException("AUTH", "Tài khoản bị vô hiệu hóa. Liên hệ quản trị viên.");
            
        if (user.Status == UserStatus.Locked)
        {
            var reasonMsg = string.IsNullOrWhiteSpace(user.LockReason) 
                ? "Tài khoản của bạn đã bị khóa bởi Quản trị viên." 
                : $"Tài khoản của bạn đã bị khóa bởi Quản trị viên. Lý do: {user.LockReason}";
            throw new BusinessRuleViolationException("LOCKED", reasonMsg);
        }

        // Verify password
        var isPasswordValid = _hasher.Verify(request.Password, user.PasswordHash ?? string.Empty);
        if (!isPasswordValid)
        {
            user.FailedLoginCount++;
            // BR-04: Lock after 5 failures
            if (user.FailedLoginCount >= BusinessConstants.MaxFailedLoginAttempts)
            {
                user.LockoutEndTime = DateTime.UtcNow.AddMinutes(BusinessConstants.LockoutMinutes);
                user.FailedLoginCount = 0;
                await _db.SaveChangesAsync(ct);
                throw new BusinessRuleViolationException("BR-04",
                    $"Đã sai {BusinessConstants.MaxFailedLoginAttempts} lần. Tài khoản bị khóa {BusinessConstants.LockoutMinutes} phút.");
            }

            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync(EventType.Login, $"Đăng nhập thất bại: {user.Email}", LogStatus.Failed,
                user.Id, ActorType.User, cancellationToken: ct);

            throw new BusinessRuleViolationException("AUTH", "Email hoặc mật khẩu không đúng.");
        }

        // Reset failed count on success
        user.FailedLoginCount = 0;
        user.LockoutEndTime = null;
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens (BR-05: RememberMe = 30 days)
        var accessToken = _tokenService.GenerateAccessToken(user, request.RememberMe);
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenHash = _tokenService.HashToken(refreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            request.RememberMe ? BusinessConstants.RememberMeTokenTtlDays : 1);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.Login, $"Đăng nhập thành công: {user.Email}",
            actorId: user.Id, actorType: ActorType.User, cancellationToken: ct);

        var expiresAt = request.RememberMe
            ? DateTime.UtcNow.AddDays(BusinessConstants.RememberMeTokenTtlDays)
            : DateTime.UtcNow.AddMinutes(BusinessConstants.AccessTokenTtlMinutes);

        return new LoginResult(
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString(),
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: expiresAt,
            FailedLoginCount: user.FailedLoginCount,
            CreatedAt: user.CreatedAt);

    }
}
