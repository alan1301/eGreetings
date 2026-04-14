using EGreetings.Domain.Common;
using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC01 - Đăng ký | UC02 - Đăng nhập | UC18 - Đăng xuất | UC19 - Hồ sơ cá nhân
/// UC22 - Quên mật khẩu | UC14 - Quản lý User (Admin)
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Inactive;

    // Email verification
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    // Password reset (UC22)
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }

    // Login lockout (BR-04): khóa 15 phút sau 5 lần sai
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }

    // Navigation
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<Greeting> Greetings { get; set; } = [];
    public ICollection<Contact> Contacts { get; set; } = [];
    public ICollection<Draft> Drafts { get; set; } = [];
    public ICollection<Feedback> Feedbacks { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
