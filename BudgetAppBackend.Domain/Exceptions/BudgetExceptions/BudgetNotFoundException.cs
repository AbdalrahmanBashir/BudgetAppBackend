using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.BudgetExceptions
{
    public class BudgetNotFoundException : DomainException
    {
        public BudgetNotFoundException(Guid budgetId)
            : base($"Budget with ID '{budgetId}' was not found.")
        {
        }
    }
}
