using EGreetings.Domain.Common;

namespace EGreetings.Domain.Events;

public record GreetingSentEvent(Guid TransactionId, Guid SenderId, string RecipientEmail) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
