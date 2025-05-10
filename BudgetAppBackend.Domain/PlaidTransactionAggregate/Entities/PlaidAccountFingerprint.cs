using System.Security.Cryptography;
using System.Text;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities
{
    public class PlaidAccountFingerprint
    {
        public UserId UserId { get; private set; }
        public string AccessToken { get; private set; }
        public string ItemId { get; private set; }
        public string Fingerprint { get; private set; }
        public List<string> AccountIds { get; private set; } = new();
        public string MaskedAccountNumbers { get; private set; }
        public string InstitutionName { get; private set; }
        public DateTime LastUpdated { get; private set; }

        // For EF Core
        private PlaidAccountFingerprint() { }

        public PlaidAccountFingerprint(
            UserId userId,
            string accessToken,
            string itemId,
            string fingerprint,
            List<string> accountIds,
            string maskedAccountNumbers,
            string institutionName)
        {
            UserId = userId;
            AccessToken = accessToken;
            ItemId = itemId;
            Fingerprint = fingerprint;
            AccountIds = accountIds;
            MaskedAccountNumbers = maskedAccountNumbers;
            InstitutionName = institutionName;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateTokenAndItemId(string accessToken, string itemId)
        {
            AccessToken = accessToken;
            ItemId = itemId;
            LastUpdated = DateTime.UtcNow;
        }

        public void AddAccountId(string accountId)
        {
            if (!AccountIds.Contains(accountId))
            {
                AccountIds.Add(accountId);
                LastUpdated = DateTime.UtcNow;
            }
        }

        public static string GenerateFingerprint(
            string institutionName,
            string maskedAccountNumbers,
            string userId)
        {
            var input = $"{institutionName}|{maskedAccountNumbers}|{userId}".ToLowerInvariant();
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }
    }
}
