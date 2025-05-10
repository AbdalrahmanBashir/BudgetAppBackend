using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities
{
    public class PlaidSyncCursor : AggregateRoot<PlaidSyncCursorId>
    {
        private PlaidSyncCursor(
            PlaidSyncCursorId id,
            UserId userId,
            string accessToken,
            string itemId,
            string cursor,
            DateTime lastSynced,
            string lastSyncStatus) : base(id)
        {
            UserId = userId;
            AccessToken = accessToken;
            ItemId = itemId;
            Cursor = cursor;
            LastSynced = lastSynced;
            LastSyncStatus = lastSyncStatus;
        }

        // For EF Core
        private PlaidSyncCursor() { }

        public UserId UserId { get; private set; }
        public string AccessToken { get; set; }
        public string ItemId { get; set; }
        public string Cursor { get; set; }
        public DateTime LastSynced { get; set; }
        public string LastSyncStatus { get; set; }

        public static PlaidSyncCursor Create(
            UserId userId,
            string accessToken,
            string itemId,
            string cursor)
        {
            return new PlaidSyncCursor(
                PlaidSyncCursorId.CreateUnique(),
                userId,
                accessToken,
                itemId,
                cursor,
                DateTime.UtcNow,
                "Success");
        }

        public void UpdateCursor(
            string cursor,
            string status = "Success")
        {
            Cursor = cursor;
            LastSynced = DateTime.UtcNow;
            LastSyncStatus = status;
        }

        public void UpdateAccessToken(string newAccessToken)
        {

            AccessToken = newAccessToken;
            LastSynced = DateTime.UtcNow;
        }
    }
}