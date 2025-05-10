using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.BudgetExceptions
{
    public class BudgetAlreadyExistsException : DomainException
    {
        public BudgetAlreadyExistsException(Guid userId, string category)
            : base($"A budget already exists for user '{userId}' and category '{category}'.")
        {
        }
    }
}
