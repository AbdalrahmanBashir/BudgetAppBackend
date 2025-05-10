namespace BudgetAppBackend.Domain.Commons
{
    // Non-generic base class for domain events
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }

    public abstract class AggregateRoot<TId> : AggregateRoot where TId : notnull
    {
        public TId Id { get; protected set; }

        protected AggregateRoot(TId id)
        {
            Id = id;
        }

        protected AggregateRoot() // For ORM frameworks
        {
            Id = default!;
        }
    }
}
