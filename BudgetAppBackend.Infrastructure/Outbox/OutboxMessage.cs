namespace BudgetAppBackend.Infrastructure.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public Guid AggregateId { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        // Constructor to initialize properties
        public OutboxMessage(Guid aggregateId, string eventType, string payload)
        {
            Id = Guid.NewGuid();
            AggregateId = aggregateId;
            EventType = eventType;
            Payload = payload;
            CreatedAt = DateTime.UtcNow;
        }

        // Parameterless constructor for EF Core
        private OutboxMessage() { }
    }
}
