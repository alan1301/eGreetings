namespace EGreetings.Domain.Common;

public abstract class HasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent ev) => _domainEvents.Add(ev);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
