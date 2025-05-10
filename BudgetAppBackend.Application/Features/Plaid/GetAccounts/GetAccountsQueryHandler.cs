using BudgetAppBackend.Application.Models.PlaidModels;
using BudgetAppBackend.Application.Service;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Application.Features.Plaid.GetAccounts
{
    public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, AccountsResponse>
    {
        private readonly IPlaidService _plaidService;
        private readonly ILogger<GetAccountsQueryHandler> _logger;

        public GetAccountsQueryHandler(IPlaidService plaidService, ILogger<GetAccountsQueryHandler> logger)
        {
            _plaidService = plaidService;
            _logger = logger;
        }

        public async Task<AccountsResponse> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting accounts for access token");
                return await _plaidService.GetAccountsAsync(request.AccessToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts");
                throw;
            }
        }
    }
}
