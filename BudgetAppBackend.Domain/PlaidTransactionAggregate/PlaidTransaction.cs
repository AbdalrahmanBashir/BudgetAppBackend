using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Domain.PlaidTransactionAggregate
{
    public sealed class PlaidTransaction : AggregateRoot<PlaidTranId>
    {
        public UserId UserId { get; private set; }
        public string PlaidTransactionId { get; private set; }
        public string AccountId { get; private set; }
        public decimal Amount { get; private set; }
        public string Name { get; private set; }
        public DateTime Date { get; private set; }

        private List<string> _categories = new();
        public IReadOnlyList<string> Categories => _categories.AsReadOnly();

        public string? CategoryId { get; private set; }
        public string? MerchantName { get; private set; }
        public bool IsRemoved { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModifiedAt { get; private set; }

        private PlaidTransaction() : base(default!) { }

        private PlaidTransaction(
            PlaidTranId id,
            UserId userId,
            string plaidTransactionId,
            string accountId,
            decimal amount,
            string name,
            DateTime date,
            List<string> categories,
            string? categoryId,
            string? merchantName)
            : base(id)
        {
            UserId = userId;
            PlaidTransactionId = plaidTransactionId;
            AccountId = accountId;
            Amount = amount;
            Name = name;
            Date = date;
            _categories = categories ?? new();
            CategoryId = categoryId;
            MerchantName = merchantName;
            IsRemoved = false;
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public static PlaidTransaction Create(
            UserId userId,
            string plaidTransactionId,
            string accountId,
            decimal amount,
            string name,
            DateTime date,
            List<string> categories,
            string? categoryId,
            string? merchantName)
        {
            var transactionId = PlaidTranId.CreateId();
            var normalizedAmount = NormalizeAmount(amount);

            var plaidTransaction = new PlaidTransaction(
                transactionId,
                userId,
                plaidTransactionId,
                accountId,
                normalizedAmount,
                name,
                date,
                categories,
                categoryId,
                merchantName
            );

            plaidTransaction.RaiseDomainEvent(new PlaidTransactionCreatedEvent(
                transactionId.Id,
                userId.Id,
                plaidTransactionId,
                accountId,
                Math.Abs(normalizedAmount),
                categories.FirstOrDefault(),
                date,
                name,
                normalizedAmount
            ));

            return plaidTransaction;
        }

        public void Update(
            decimal amount,
            string name,
            DateTime date,
            List<string> categories,
            string? categoryId,
            string? merchantName)
        {
            var oldAmount = Amount;
            var oldCategories = new List<string>(_categories);

            Amount = NormalizeAmount(amount);
            Name = name;
            _categories = categories ?? new();
            CategoryId = categoryId;
            MerchantName = merchantName;
            LastModifiedAt = DateTime.UtcNow;

            if (oldAmount != Amount || !oldCategories.SequenceEqual(_categories))
            {
                RaiseDomainEvent(new PlaidTransactionModifiedDomainEvent(
                    Id.Id,
                    UserId.Id,
                    oldAmount,
                    Math.Abs(Amount),
                    oldCategories.FirstOrDefault(),
                    _categories.FirstOrDefault(),
                    Date
                ));
            }
        }

        public void MarkAsRemoved()
        {
            if (!IsRemoved)
            {
                IsRemoved = true;
                LastModifiedAt = DateTime.UtcNow;

                RaiseDomainEvent(new PlaidTransactionRemovedDomainEvent(
                    Id.Id,
                    UserId.Id,
                    Math.Abs(Amount),
                    Categories.FirstOrDefault(),
                    Date
                ));
            }
        }

        private static decimal NormalizeAmount(decimal amount)
        {
            return amount > 0 ? -amount : Math.Abs(amount);
        }
    }
}
