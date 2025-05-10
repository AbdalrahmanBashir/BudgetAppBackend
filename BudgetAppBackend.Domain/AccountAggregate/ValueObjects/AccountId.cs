using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.AccountAggregate.ValueObjects
{
    public sealed class AccountId : ValueObject
    {
        public Guid Id { get; private set; }

        private AccountId(Guid id)
        {
            Id = id;
        }


        public static AccountId CreateId()
        {
            return new AccountId(Guid.NewGuid());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public static AccountId Create(Guid? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "Guid value cannot be null.");
            }
            return new AccountId(id.Value);
        }
    }
}
