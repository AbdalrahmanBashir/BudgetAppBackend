using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects
{
    public class PlaidTranId : ValueObject
    {
        public Guid Id { get; private set; }

        private PlaidTranId(Guid id)
        {
            Id = id;
        }


        public static PlaidTranId CreateId()
        {
            return new PlaidTranId(Guid.NewGuid());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public static PlaidTranId Create(Guid? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "Guid value cannot be null.");
            }
            return new PlaidTranId(id.Value);
        }
    }
}
