namespace BudgetAppBackend.Domain.Commons
{

    public abstract class Entity
    {
        // Base non-generic entity functionality
    }
    public abstract class Entity<TId> : IEquatable<Entity<TId>>
         where TId : notnull
    {

        public TId? Id { get; private set; }

        public Entity(TId? id)
        {
            Id = id;
        }
        private Entity()
        {
            // It's mainly used for ORM frameworks that require a parameterless constructor.
        }

        public override bool Equals(object? obj) => obj is Entity<TId> entity && Id!.Equals(entity.Id);

        public static bool operator ==(Entity<TId> left, Entity<TId>? right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(Entity<TId> left, Entity<TId> right)
        {
            return Equals(left, right);
        }
        public override int GetHashCode()
        {
            return Id!.GetHashCode();
        }

        public bool Equals(Entity<TId>? other)
        {
            return Equals((object?)other);
        }
    }
}
