using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.DomainEvents
{
    public sealed record TransactionUpdatedEvent(Guid UserId, string category, decimal OldAmount, decimal NewAmount, DateTime Date) : IDomainEvent
    {
    }
}
