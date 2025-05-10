using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Domain.UserAggregate.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public UserId? UserId { get; private set; }
        public string TokenHash { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; private set; }
        public bool IsRevoked => RevokedAt.HasValue;

        private RefreshToken()
        {
            // It's mainly used for ORM frameworks that require a parameterless constructor.
        }

        public RefreshToken(UserId userId, string tokenHash, DateTime expiryDate)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            TokenHash = tokenHash;
            ExpiryDate = expiryDate;
        }

        public void Revoke()
        {
            if (!IsRevoked)
            {
                RevokedAt = DateTime.UtcNow;
            }
        }
    }
}
