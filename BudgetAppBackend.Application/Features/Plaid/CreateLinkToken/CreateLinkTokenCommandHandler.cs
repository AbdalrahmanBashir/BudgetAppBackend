using BudgetAppBackend.Application.Models.PlaidModels;
using BudgetAppBackend.Application.Service;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Application.Features.Plaid.CreateLinkToken
{
    public class CreateLinkTokenCommandHandler : IRequestHandler<CreateLinkTokenCommand, LinkTokenResponse>
    {
        private readonly IPlaidService _plaidService;
        private readonly ILogger<CreateLinkTokenCommandHandler> _logger;

        public CreateLinkTokenCommandHandler(IPlaidService plaidService, ILogger<CreateLinkTokenCommandHandler> logger)
        {
            _plaidService = plaidService;
            _logger = logger;
        }

        public async Task<LinkTokenResponse> Handle(CreateLinkTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating link token for user {UserId}", request.ClientUserId);
                return await _plaidService.CreateLinkTokenAsync(request.ClientUserId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating link token for user {UserId}", request.ClientUserId);
                throw;
            }
        }
    }
}
