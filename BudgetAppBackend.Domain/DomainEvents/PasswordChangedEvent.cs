using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.DomainEvents
{
    public sealed record PasswordChangedEvent(Guid UserId, string LastName, string Email) : IDomainEvent;

}
