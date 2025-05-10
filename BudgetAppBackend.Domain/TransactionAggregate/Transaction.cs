using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.TransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using BudgetAppBackend.Domain.Exceptions.TransactionExceptions;

namespace BudgetAppBackend.Domain.TransactionAggregate
{
    public sealed class Transaction : AggregateRoot<TransactionId>
    {
        public UserId UserId { get; private set; }
        private List<string> _categories = new();

        public IReadOnlyList<string> Categories => _categories.AsReadOnly();

        public decimal Amount { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public string Payee { get; private set; }
        public TransactionType Type { get; private set; } //New Property

        private Transaction() : base(default!) { }

        private Transaction(TransactionId id, UserId userId, List<string> categories, decimal amount, DateTime transactionDate, string payee, TransactionType type)
            : base(id)
        {

            UserId = userId;
            _categories = categories;
            Amount = amount;
            TransactionDate = transactionDate;
            CreatedDate = DateTime.UtcNow;
            Payee = payee;
            Type = type;
        }

        public static Transaction Create(UserId userId, List<string> newCategories, decimal amount, DateTime transactionDate, string payee, TransactionType type)
        {
            if (string.IsNullOrWhiteSpace(payee))
                throw new TransactionPayeeException("Payee name cannot be empty.");
            var transaction = new Transaction(TransactionId.CreateId(), userId, newCategories, amount, transactionDate, payee, type);
            transaction.RaiseDomainEvent(new TransactionCreatedEvent(userId.Id, newCategories.FirstOrDefault(), amount, transactionDate, payee, type));
            return transaction;
        }

        public void UpdateTransaction(decimal newAmount, DateTime newTransactionDate, string newPayee, string category, TransactionType newType)
        {

            if (string.IsNullOrWhiteSpace(newPayee))
                throw new TransactionPayeeException("Payee name cannot be empty.");

            var oldAmount = Amount;
            var oldType = Type;
            var oldCategory = _categories.FirstOrDefault();

            _categories = new List<string> { category };

            Amount = newAmount;
            TransactionDate = newTransactionDate;
            Payee = newPayee;
            Type = newType;
            

            RaiseDomainEvent(new TransactionUpdatedEvent(UserId.Id, category, oldAmount, newAmount, TransactionDate));
        }

        public void DeleteTransaction()
        {
            RaiseDomainEvent(new TransactionDeletedEvent(UserId.Id, _categories.FirstOrDefault(), Amount, TransactionDate));
        }
    }
}
