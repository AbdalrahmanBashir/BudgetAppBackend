using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.TransactionExceptions
{
    public class TransactionAmountException : DomainException
    {
        public TransactionAmountException(string message) : base(message)
        {
        }
    }
}
