using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.DomainEvents;
public record PlaidTransactionCreatedEvent(
    Guid TransactionId,
    Guid UserId,
    string PlaidTransactionId,
    string AccountId,
    decimal Amount,
    string Category,
    DateTime Date,
    string Name,
    decimal normalizedAmount) : IDomainEvent;

