namespace EGreetings.Domain.Enums;

public enum UserRole
{
    User,
    Admin
}

public enum UserStatus
{
    PendingActivation,
    Active,
    Disabled,
    Locked
}

public enum SocialProvider
{
    None,
    Google,
    Facebook
}

public enum CardStatus
{
    Active,
    Inactive  // "Ẩn" – BR-19
}

public enum CategoryStatus
{
    Active,
    Hidden
}

public enum SubscriptionStatus
{
    Pending,   // Chờ xác nhận thanh toán
    Active,    // Đang hoạt động
    Expired,   // Hết hạn
    Disabled   // Bị vô hiệu hóa – KHÔNG thể gia hạn
}

public enum PaymentMethod
{
    Gateway,      // VNPay / MoMo / PayPal
    BankTransfer  // Chuyển khoản thủ công
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed
}

public enum ContactGroup
{
    Family,
    Friends,
    Colleagues
}

public enum TransactionStatus
{
    Pending,
    Sent,
    Failed,
    Scheduled
}

public enum FeedbackStatus
{
    Unread,
    Read
}

public enum RetryStatus
{
    PendingRetry,
    Sent,
    Failed
}

public enum ActorType
{
    User,
    Admin,
    System
}

public enum EventType
{
    Login,
    Logout,
    Payment,
    SendCard,
    AdminAction,
    SystemError,
    Register,
    PasswordReset,
    SubscriptionChange,
    JobRun
}

public enum LogStatus
{
    Success,
    Failed
}
