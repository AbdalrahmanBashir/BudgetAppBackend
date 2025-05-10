using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.UserAggregate.ValueObjects
{
    public sealed class UserId : ValueObject
    {
        public Guid Id { get; private set; }

        private UserId(Guid id)
        {
            Id = id;
        }


        public static UserId CreateId()
        {
            return new UserId(Guid.NewGuid());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public static UserId Create(Guid? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "Guid value cannot be null.");
            }
            return new UserId(id.Value);
        }
    }
}
