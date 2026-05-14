using EGreetings.Domain.Common;
using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Events;

public record SubscriptionActivatedEvent(Guid SubscriptionId, Guid UserId, SubscriptionPlan Plan) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
