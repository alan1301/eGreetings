using EGreetings.Domain.Common;

namespace EGreetings.Domain.Events;

public record UserRegisteredEvent(Guid UserId, string Email, string FullName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
