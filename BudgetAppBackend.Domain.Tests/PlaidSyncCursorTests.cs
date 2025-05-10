using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Domain.Tests
{
    public class PlaidSyncCursorTests
    {
        private readonly UserId _userId = UserId.Create(Guid.NewGuid());
        private readonly PlaidSyncCursorId _plaidSyncCursorId = PlaidSyncCursorId.CreateUnique();
        private readonly string _accessToken = "initial_access_token";
        private readonly string _itemId = "initial_item_id";
        private readonly string _initialCursor = "initial_cursor";

        [Fact]
        public void Create_PlaidSyncCursor_Success()
        {
            // Arrange  
            // Act  
            PlaidSyncCursor cursor = PlaidSyncCursor.Create(_userId, _accessToken, _itemId, _initialCursor);

            // Assert  
            Assert.NotNull(cursor);
            Assert.NotEqual(_plaidSyncCursorId, cursor.Id);
            Assert.Equal(_userId, cursor.UserId);
            Assert.Equal(_accessToken, cursor.AccessToken);
            Assert.Equal(_itemId, cursor.ItemId);
            Assert.Equal(_initialCursor, cursor.Cursor);
            Assert.NotEqual(default(DateTime), cursor.LastSynced);
            Assert.Equal("Success", cursor.LastSyncStatus);
        }

        [Fact]
        public void UpdateCursor_UpdatesCursorAndLastSynced()
        {
            // Arrange  
            PlaidSyncCursor cursor = PlaidSyncCursor.Create(_userId, _accessToken, _itemId, _initialCursor);
            string newCursor = "new_cursor_value";
            DateTime initialLastSynced = cursor.LastSynced;

            // Act  
            cursor.UpdateCursor(newCursor);

            // Assert  
            Assert.Equal(newCursor, cursor.Cursor);
            Assert.NotEqual(initialLastSynced, cursor.LastSynced);
            Assert.Equal("Success", cursor.LastSyncStatus);
        }

        [Fact]
        public void UpdateCursor_WithStatus_UpdatesCursorAndLastSyncedAndStatus()
        {
            // Arrange  
            PlaidSyncCursor cursor = PlaidSyncCursor.Create(_userId, _accessToken, _itemId, _initialCursor);
            string newCursor = "new_cursor_value";
            string newStatus = "Failed";
            DateTime initialLastSynced = cursor.LastSynced;

            // Act  
            cursor.UpdateCursor(newCursor, newStatus);

            // Assert  
            Assert.Equal(newCursor, cursor.Cursor);
            Assert.NotEqual(initialLastSynced, cursor.LastSynced);
            Assert.Equal(newStatus, cursor.LastSyncStatus);
        }

        [Fact]
        public void UpdateAccessToken_UpdatesAccessTokenAndLastSynced()
        {
            // Arrange  
            PlaidSyncCursor cursor = PlaidSyncCursor.Create(_userId, _accessToken, _itemId, _initialCursor);
            string newAccessToken = "new_access_token";
            DateTime initialLastSynced = cursor.LastSynced;

            // Act  
            cursor.UpdateAccessToken(newAccessToken);

            // Assert  
            Assert.Equal(newAccessToken, cursor.AccessToken);
            Assert.NotEqual(initialLastSynced, cursor.LastSynced);
        }

        
    }
}
