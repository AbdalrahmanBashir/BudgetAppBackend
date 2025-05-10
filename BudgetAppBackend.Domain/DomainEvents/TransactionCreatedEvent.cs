using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.TransactionAggregate;

namespace BudgetAppBackend.Domain.DomainEvents
{
    public sealed record TransactionCreatedEvent(Guid UserId, string category, decimal amount, DateTime Date, string payee, TransactionType Type) : IDomainEvent
    {

    }
}
