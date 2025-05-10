using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.TransactionExceptions
{
    public class TransactionPayeeException : DomainException
    {
        public TransactionPayeeException(string message) : base(message)
        {

        }
    }
}
