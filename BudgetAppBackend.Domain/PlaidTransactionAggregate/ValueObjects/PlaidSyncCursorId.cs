using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects
{
    public class PlaidSyncCursorId : ValueObject
    {
        public string Id { get; private set; }

        private PlaidSyncCursorId(string value)
        {
            Id = value;
        }

        public static PlaidSyncCursorId Create(string value)
        {
            return new PlaidSyncCursorId(value);
        }

        public static PlaidSyncCursorId CreateUnique()
        {
            return new PlaidSyncCursorId(Guid.NewGuid().ToString());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}