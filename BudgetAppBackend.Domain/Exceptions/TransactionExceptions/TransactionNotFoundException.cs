using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.TransactionExceptions
{
    public class TransactionNotFoundException : DomainException
    {
        public TransactionNotFoundException(string message) : base(message)
        {
        }
    }
}
