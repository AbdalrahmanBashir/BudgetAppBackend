using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Application.Contracts
{
    public interface IPlaidAccountFingerprintRepository
    {
        Task<PlaidAccountFingerprint?> GetByInstitutionAndAccountsAsync(
            UserId userId,
            string institutionId,
            List<(string name, string mask)> accounts);

        Task<PlaidAccountFingerprint?> GetByItemIdAsync(
            UserId userId,
            string itemId);

        Task<List<PlaidAccountFingerprint>> GetByUserIdAsync(
            UserId userId);

        Task SaveFingerprintAsync(
            PlaidAccountFingerprint fingerprint);

        Task UpdateFingerprintAsync(
            PlaidAccountFingerprint fingerprint);

        Task DeleteFingerprintAsync(
            UserId userId,
            string itemId);
    }
}
