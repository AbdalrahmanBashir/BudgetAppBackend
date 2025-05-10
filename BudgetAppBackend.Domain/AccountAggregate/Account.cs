using BudgetAppBackend.Domain.AccountAggregate.ValueObjects;
using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Domain.AccountAggregate
{
    public class Account : AggregateRoot<AccountId>
    {
        public UserId OwnerId { get; private set; }
        public AccountType Type { get; private set; }
        public string? PlaidAccountId { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Parameterless constructor for ORM frameworks.
        private Account() : base(default!)
        {
        }

        private Account(AccountId id, UserId ownerId, AccountType type, string? plaidAccountId, decimal initialBalance) : base(id)
        {
            OwnerId = ownerId ?? throw new ArgumentNullException(nameof(ownerId));
            Type = type;
            if (type == AccountType.External)
            {
                PlaidAccountId = ValidateString(plaidAccountId, nameof(PlaidAccountId));
            }
            else
            {
                PlaidAccountId = null;
            }
            Balance = initialBalance;
            CreatedAt = DateTime.UtcNow;
        }

        public static Account CreateNewExternalAccount(UserId ownerId, string plaidAccountId, decimal initialBalance = 0)
        {
            var account = new Account(
                AccountId.CreateId(),
                ownerId,
                AccountType.External,
                plaidAccountId,
                initialBalance
            );

            // account.RaiseDomainEvent(new ExternalAccountCreatedEvent(account.Id.Id, ownerId.Id, plaidAccountId, initialBalance));
            return account;
        }

        public static Account CreateNewInternalCashFlowAccount(UserId ownerId, decimal initialBalance = 0)
        {
            var account = new Account(
                AccountId.CreateId(),
                ownerId,
                AccountType.InternalCashFlow,
                null,
                initialBalance
            );

            // account.RaiseDomainEvent(new InternalCashFlowAccountCreatedEvent(account.Id.Id, ownerId.Id, initialBalance));
            return account;
        }

        private static string ValidateString(string? value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{propertyName} cannot be null or empty.", propertyName);
            return value;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive.", nameof(amount));

            Balance += amount;
           // RaiseDomainEvent(new AccountDepositedEvent(Id.Id, OwnerId.Id, amount, Balance));
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive.", nameof(amount));

            if (amount > Balance)
                throw new InvalidOperationException("Insufficient funds.");

            Balance -= amount;
            //RaiseDomainEvent(new AccountWithdrawnEvent(Id.Id, OwnerId.Id, amount, Balance));
        }

        public void UpdateCashFlow(decimal amount)
        {
            Balance += amount;
            //RaiseDomainEvent(new AccountCashFlowUpdatedEvent(Id.Id, OwnerId.Id, amount, Balance));
        }
    }

    public enum AccountType
    {
        External,
        InternalCashFlow
    }

}
