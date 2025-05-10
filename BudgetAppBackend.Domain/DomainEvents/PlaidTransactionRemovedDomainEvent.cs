using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.DomainEvents;

public record PlaidTransactionRemovedDomainEvent(
    Guid TransactionId,
    Guid UserId,
    decimal Amount,
    string Category,
    DateTime Date) : IDomainEvent;  

