using BudgetAppBackend.Application.Models.PlaidModels;

namespace BudgetAppBackend.Application.Service
{
    public interface IPlaidService
    {
        Task<LinkTokenResponse> CreateLinkTokenAsync(string clientUserId, CancellationToken cancellationToken = default);
        Task<AccessTokenResponse> ExchangePublicTokenAsync(string publicToken, CancellationToken cancellationToken = default);
        Task<AccountsResponse> GetAccountsAsync(string accessToken, CancellationToken cancellationToken = default);
        Task<TransactionsSyncResponse> SyncTransactionsAsync(Guid userId, string accessToken, string? cursor = null, int count = 100, CancellationToken cancellationToken = default);
    }
}
