using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.BudgetExceptions
{
    public class BudgetInvalidAmountException : DomainException
    {
        public BudgetInvalidAmountException(decimal amount, string message)
            : base($"Invalid amount ({amount}). {message}")
        {
        }
    }
}
