using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.DomainEvents;

public record  PlaidTransactionModifiedDomainEvent(
    Guid TransactionId,
    Guid UserId,
    decimal oldAmount,
    decimal Amount,
    string  oldCategory, 
    string  Category, 
    DateTime Date): IDomainEvent;
