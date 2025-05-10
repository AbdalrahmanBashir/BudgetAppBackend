using BudgetAppBackend.Application.Configuration;
using BudgetAppBackend.Application.Models.PlaidModels;
using BudgetAppBackend.Application.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace BudgetAppBackend.Infrastructure.Services
{
    public class PlaidService : IPlaidService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PlaidService> _logger;
        private readonly PlaidOptions _options;

        public PlaidService(IOptions<PlaidOptions> options, ILogger<PlaidService> logger)
        {
            _options = options.Value;
            _logger = logger;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_options.BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<LinkTokenResponse> CreateLinkTokenAsync(string clientUserId, CancellationToken cancellationToken = default)
        {


            var request = new CreateLinkTokenRequest
            {
                ClientId = _options.ClientId!,
                Secret = _options.Secret!,
                ClientName = "Budget App",
                User = new PlaidUser { ClientUserId = clientUserId },
                Products = new List<string> { "auth", "transactions" },
                CountryCodes = new List<string> { "US" },
                Language = "en",
                AccountFilters = new
                {
                    depository = new
                    {
                        account_subtypes = new[] { "checking", "savings" }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("link/token/create", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            return new LinkTokenResponse(
                result.GetProperty("link_token").GetString()!,
                result.GetProperty("request_id").GetString()!,
                DateTime.UtcNow.AddHours(4));
        }

        public async Task<AccessTokenResponse> ExchangePublicTokenAsync(string publicToken, CancellationToken cancellationToken = default)
        {
            var request = new
            {
                client_id = _options.ClientId,
                secret = _options.Secret,
                public_token = publicToken
            };

            var response = await _httpClient.PostAsJsonAsync("item/public_token/exchange", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            return new AccessTokenResponse(
                result.GetProperty("access_token").GetString()!,
                result.GetProperty("item_id").GetString()!,
                result.GetProperty("request_id").GetString()!);
        }

        public async Task<AccountsResponse> GetAccountsAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            var request = new
            {
                client_id = _options.ClientId,
                secret = _options.Secret,
                access_token = accessToken
            };

            var response = await _httpClient.PostAsJsonAsync("accounts/get", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            var accounts = result.GetProperty("accounts").EnumerateArray()
                .Select(a => new PlaidAccount(
                    a.GetProperty("account_id").GetString()!,
                    a.GetProperty("name").GetString()!,
                    a.GetProperty("type").GetString()!,
                    a.GetProperty("subtype").GetString()!,
                    a.GetProperty("balances").GetProperty("current").GetDecimal(),
                    a.GetProperty("balances").GetProperty("available").ValueKind != JsonValueKind.Null
                        ? a.GetProperty("balances").GetProperty("available").GetDecimal()
                        : 0))
                .ToList();

            return new AccountsResponse(accounts, result.GetProperty("request_id").GetString()!);
        }

        public async Task<TransactionsSyncResponse> SyncTransactionsAsync(Guid userId, string accessToken, string? cursor = null, int count = 100, CancellationToken cancellationToken = default)
        {
            var request = new
            {
                client_id = _options.ClientId,
                secret = _options.Secret,
                access_token = accessToken,
                cursor = cursor,
                count = Math.Min(500, Math.Max(1, count))
            };

            var response = await _httpClient.PostAsJsonAsync("transactions/sync", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

            var added = result.GetProperty("added").EnumerateArray()
                .Select(t => ParseTransaction(t, userId))
                .ToList();

            var modified = result.GetProperty("modified").EnumerateArray()
                .Select(t => ParseTransaction(t, userId))
                .ToList();

            var removed = result.GetProperty("removed").EnumerateArray()
                .Select(t => new RemovedTransaction(
                    t.GetProperty("transaction_id").GetString()!,
                    t.GetProperty("account_id").GetString()!))
                .ToList();
            var nextCursor = result.GetProperty("next_cursor").GetString()!;
            var hasMore = result.GetProperty("has_more").GetBoolean();
            var requestId = result.GetProperty("request_id").GetString()!;

            if (!string.IsNullOrEmpty(accessToken))
            {
                var itemRequest = new
                {
                    client_id = _options.ClientId,
                    secret = _options.Secret,
                    access_token = accessToken
                };

                var itemResponse = await _httpClient.PostAsJsonAsync("item/get", itemRequest, cancellationToken);
                itemResponse.EnsureSuccessStatusCode();
                var itemResult = await itemResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                var itemId = itemResult.GetProperty("item").GetProperty("item_id").GetString()!;

                return new TransactionsSyncResponse(
                    added,
                    modified,
                    removed,
                    nextCursor,
                    hasMore,
                    requestId,
                    itemId);
            }
            return new TransactionsSyncResponse(
                added,
                modified,
                removed,
                nextCursor,
                hasMore,
                requestId);
        }

        private static PlaidTransactionDto ParseTransaction(JsonElement t, Guid userId)
        {
            var personalFinanceCategory = t.TryGetProperty("personal_finance_category", out var pfcElement) && pfcElement.ValueKind != JsonValueKind.Null
                ? new PersonalFinanceCategory(
                    pfcElement.GetProperty("primary").GetString()!,
                    pfcElement.GetProperty("detailed").GetString()!,
                    pfcElement.TryGetProperty("confidence_level", out var confLevel) ? confLevel.GetString() : null)
                : null;

            var iconUrl = t.TryGetProperty("personal_finance_category_icon_url", out var iconElement) && iconElement.ValueKind == JsonValueKind.String
                    ? iconElement.GetString()
                    : null;

            var counterparties = t.TryGetProperty("counterparties", out var counterpartiesElement) && counterpartiesElement.ValueKind != JsonValueKind.Array
                ? counterpartiesElement.EnumerateArray()
                    .Select(c => new Counterparty(
                        c.GetProperty("name").GetString()!,
                        c.GetProperty("type").GetString()!,
                        c.TryGetProperty("entity_id", out var entityId) ? entityId.GetString() : null,
                        c.TryGetProperty("website", out var website) ? website.GetString() : null,
                        c.TryGetProperty("logo_url", out var logoUrl) ? logoUrl.GetString() : null,
                        c.TryGetProperty("confidence_level", out var confLevel) ? confLevel.GetString() : null))
                    .ToList()
                : new List<Counterparty>();


            return new PlaidTransactionDto(
                        userId: userId,
                        PlaidTransactionId: t.GetProperty("transaction_id").GetString()!,
                        AccountId: t.GetProperty("account_id").GetString()!,
                        Amount: t.GetProperty("amount").GetDecimal(),
                        Name: t.GetProperty("name").GetString()!,
                        Date: DateTime.Parse(t.GetProperty("date").GetString()!).ToUniversalTime(),
                        Categories: t.TryGetProperty("category", out var categoryElement) && categoryElement.ValueKind == JsonValueKind.Array
                        ? categoryElement.EnumerateArray().Select(c => c.GetString()!).ToList()
                        : new List<string>(),

                        CategoryId: t.TryGetProperty("category_id", out var categoryId)
                            ? categoryId.GetString()
                            : null,
                        MerchantName: t.TryGetProperty("merchant_name", out var merchantName)
                            ? merchantName.GetString()
                            : null
            );
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
