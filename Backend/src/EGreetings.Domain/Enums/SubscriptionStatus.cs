namespace EGreetings.Domain.Enums;

public enum SubscriptionStatus
{
    Pending,   // Awaiting payment confirmation
    Active,    // Currently active
    Expired,   // Subscription has expired
    Disabled   // Disabled – cannot be renewed
}
