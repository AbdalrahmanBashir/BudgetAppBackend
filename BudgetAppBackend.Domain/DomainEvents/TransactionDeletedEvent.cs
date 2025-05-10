using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.DomainEvents
{
    public sealed record TransactionDeletedEvent(Guid UserId, string category, decimal Amount, DateTime Date) : IDomainEvent
    {
    }
}

