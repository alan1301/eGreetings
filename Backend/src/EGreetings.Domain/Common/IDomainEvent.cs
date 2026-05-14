using MediatR;

namespace EGreetings.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}
