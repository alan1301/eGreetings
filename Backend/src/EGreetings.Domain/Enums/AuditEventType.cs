namespace EGreetings.Domain.Enums;

/// <summary>
/// Loại sự kiện audit - UC30 (Logging & Audit) - BR-33
/// </summary>
public enum AuditEventType
{
    // Nhóm 1: Xác thực
    UserRegistered = 1,
    UserLoggedIn = 2,
    UserLoggedOut = 3,
    PasswordReset = 4,
    EmailVerified = 5,

    // Nhóm 2: Thiệp
    GreetingCreated = 10,
    GreetingSent = 11,
    GreetingScheduled = 12,
    GreetingCancelled = 13,

    // Nhóm 3: Thanh toán & Subscribe
    SubscriptionCreated = 20,
    SubscriptionRenewed = 21,
    SubscriptionExpired = 22,
    SubscriptionDisabled = 23,
    PaymentCompleted = 24,
    PaymentFailed = 25,

    // Nhóm 4: Quản trị
    UserBanned = 30,
    UserUnbanned = 31,
    CategoryCreated = 32,
    CategoryUpdated = 33,
    CategoryDeleted = 34,
    TemplateCreated = 35,
    TemplateUpdated = 36,
    TemplateDeleted = 37,
    TemplateHidden = 39,
    WebContentUpdated = 38,
    WebContentRollback = 45,
    PaymentConfirmed = 46,
    FeedbackReplied = 47,

    // Nhóm 5: Hệ thống
    EmailRetried = 40,
    EmailFailed = 41,
    AutoSendTriggered = 42,
    SystemError = 43
}
