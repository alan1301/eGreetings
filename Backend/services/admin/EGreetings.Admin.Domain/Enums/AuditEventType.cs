namespace EGreetings.Admin.Domain.Enums;

public enum AuditEventType
{
    // User events
    UserRegistered = 1,
    UserLoggedIn = 2,
    UserBanned = 3,

    // Greeting events
    GreetingSent = 11,
    TemplateCreated = 12,
    TemplateHidden = 13,

    // Subscription events
    SubscriptionCreated = 21,
    PaymentConfirmed = 22,
    SubscriptionDisabled = 23,

    // Feedback events
    FeedbackReplied = 31,

    // Content events
    WebContentUpdated = 41,
    WebContentRolledBack = 42
}
