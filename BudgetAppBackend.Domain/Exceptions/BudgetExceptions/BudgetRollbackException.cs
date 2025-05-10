using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.BudgetExceptions
{
    public class BudgetRollbackException : DomainException
    {
        public BudgetRollbackException(decimal rollbackAmount, decimal spentAmount)
        : base($"Cannot rollback {rollbackAmount} when only {spentAmount} was spent.")
        {
        }
    }
}
