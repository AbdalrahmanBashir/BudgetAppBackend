using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.DomainEvents
{
    public sealed record EmailVerificationCodeGeneratedEvent(string code, DateTime expiry, string firstName, string lastName, string email) : IDomainEvent
    {
        
       
    }

}
