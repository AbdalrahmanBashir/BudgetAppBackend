using System.Text.Json.Serialization;
using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.TransactionAggregate.ValueObjects
{
    public sealed class TransactionId : ValueObject
    {
        public Guid Id { get; private set; }

        [JsonConstructor]
        private TransactionId(Guid id)
        {
            Id = id;
        }


        public static TransactionId CreateId()
        {
            return new TransactionId(Guid.NewGuid());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public static TransactionId Create(Guid? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "Guid value cannot be null.");
            }
            return new TransactionId(id.Value);
        }
    }
}
