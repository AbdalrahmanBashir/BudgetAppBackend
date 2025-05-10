using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Domain.Tests
{
    public class PlaidAccountFingerprintTests
    {
        private readonly UserId _userId = UserId.Create(Guid.NewGuid());
        private const string AccessToken = "access_123";
        private const string ItemId = "item_456";
        private const string InstitutionName = "Test Bank";
        private const string MaskedNumbers = "****1234";
        private readonly List<string> _accountIds = new() { "acc1", "acc2" };

        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange & Act
            var fingerprint = new PlaidAccountFingerprint(
                _userId,
                AccessToken,
                ItemId,
                "fingerprint",
                _accountIds,
                MaskedNumbers,
                InstitutionName);

            // Assert
            Assert.Equal(_userId, fingerprint.UserId);
            Assert.Equal(AccessToken, fingerprint.AccessToken);
            Assert.Equal(ItemId, fingerprint.ItemId);
            Assert.Equal("fingerprint", fingerprint.Fingerprint);
            Assert.Equal(_accountIds, fingerprint.AccountIds);
            Assert.Equal(MaskedNumbers, fingerprint.MaskedAccountNumbers);
            Assert.Equal(InstitutionName, fingerprint.InstitutionName);
            Assert.InRange(fingerprint.LastUpdated,
                DateTime.UtcNow.AddSeconds(-1),
                DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void UpdateTokenAndItemId_UpdatesPropertiesAndTimestamp()
        {
            // Arrange
            var original = new PlaidAccountFingerprint(
                _userId,
                "old_token",
                "old_item",
                "fp",
                _accountIds,
                MaskedNumbers,
                InstitutionName);

            var originalDate = original.LastUpdated;

            // Act
            original.UpdateTokenAndItemId("new_token", "new_item");

            // Assert
            Assert.Equal("new_token", original.AccessToken);
            Assert.Equal("new_item", original.ItemId);
            Assert.True(original.LastUpdated > originalDate);
            Assert.Equal(_accountIds, original.AccountIds);
            Assert.Equal(InstitutionName, original.InstitutionName);
        }

        [Fact]
        public void AddAccountId_AddsNewAccountIdAndUpdatesTimestamp()
        {
            // Arrange
            var fingerprint = CreateTestFingerprint();
            var originalDate = fingerprint.LastUpdated;

            // Act
            fingerprint.AddAccountId("new_account");

            // Assert
            Assert.Contains("new_account", fingerprint.AccountIds);
            Assert.True(fingerprint.LastUpdated > originalDate);
        }

        [Fact]
        public void AddAccountId_DoesNotAddDuplicateAccountId()
        {
            // Arrange
            var fingerprint = CreateTestFingerprint();
            var originalDate = fingerprint.LastUpdated;

            // Act
            fingerprint.AddAccountId("acc1"); // Already exists

            // Assert
            Assert.Equal(2, fingerprint.AccountIds.Count);
            Assert.Equal(originalDate, fingerprint.LastUpdated);
        }

        [Fact]
        public void GenerateFingerprint_CreatesConsistentHash()
        {
            // Arrange
            const string userId = "user123";
            const string institution = "Bank";
            const string masked = "****5678";

            // Act
            var fp1 = PlaidAccountFingerprint.GenerateFingerprint(institution, masked, userId);
            var fp2 = PlaidAccountFingerprint.GenerateFingerprint(institution, masked, userId);

            // Assert
            Assert.Equal(fp1, fp2);
        }

        [Fact]
        public void GenerateFingerprint_IsCaseInsensitive()
        {
            // Arrange
            const string userId = "User123";
            const string institution = "bank";
            const string masked = "****5678";

            // Act
            var fpLower = PlaidAccountFingerprint.GenerateFingerprint(institution.ToLower(), masked, userId.ToLower());
            var fpUpper = PlaidAccountFingerprint.GenerateFingerprint(institution.ToUpper(), masked, userId.ToUpper());

            // Assert
            Assert.Equal(fpLower, fpUpper);
        }

        private PlaidAccountFingerprint CreateTestFingerprint() => new(
            _userId,
            AccessToken,
            ItemId,
            "test_fp",
            new List<string> { "acc1", "acc2" },
            MaskedNumbers,
            InstitutionName);
    }
}