using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.BudgetAggregate.ValueObjects
{
    public sealed class BudgetId : ValueObject
    {
        public Guid Id { get; private set; }

        private BudgetId(Guid id)
        {
            Id = id;
        }


        public static BudgetId CreateId()
        {
            return new BudgetId(Guid.NewGuid());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public static BudgetId Create(Guid? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "Guid value cannot be null.");
            }
            return new BudgetId(id.Value);
        }
    }
}
