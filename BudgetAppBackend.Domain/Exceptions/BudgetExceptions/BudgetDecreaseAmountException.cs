using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.BudgetExceptions
{
    public class BudgetDecreaseAmountException : DomainException
    {
        public BudgetDecreaseAmountException(decimal newAmount, decimal spentAmount)
        : base($"New total amount ({newAmount}) cannot be less than the already spent amount ({spentAmount}).")
        {
        }
    }
}
