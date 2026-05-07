using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC01, UC02, UC18, UC19, UC22 – User account entity.
/// BR-01: PasswordHash via BCrypt. BR-02: Email unique. BR-03: EmailVerificationToken TTL 24h.
/// BR-04: FailedLoginCount + LockoutEndTime. BR-05: RememberMe. BR-26: PasswordResetToken TTL 15min.
/// </summary>
public class User : BaseAuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;                // BR-02: unique index
    public string? PasswordHash { get; set; }                        // BR-01: BCrypt

    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.PendingActivation;
    public string? LockReason { get; set; }

    // Email verification (BR-03)
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }       // TTL 24h

    // Password reset (BR-26)
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }           // TTL 15 min
    public bool PasswordResetTokenUsed { get; set; } = false;

    // Login lockout (BR-04)
    public int FailedLoginCount { get; set; } = 0;
    public DateTime? LockoutEndTime { get; set; }

    // Refresh token (BR-05, BR-25)
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Social login
    public SocialProvider SocialProvider { get; set; } = SocialProvider.None;
    public string? SocialProviderId { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Draft> Drafts { get; set; } = new List<Draft>();
    public ICollection<GreetingTransaction> SentTransactions { get; set; } = new List<GreetingTransaction>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
