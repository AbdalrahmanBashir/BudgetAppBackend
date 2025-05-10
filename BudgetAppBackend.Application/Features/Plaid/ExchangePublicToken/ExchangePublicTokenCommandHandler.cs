using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Application.Features.Plaid.ExchangePublicToken
{
    public class ExchangePublicTokenCommandHandler : IRequestHandler<ExchangePublicTokenCommand, LinkSuccessResult>
    {
        private readonly IPlaidService _plaidService;
        private readonly IPlaidAccountFingerprintRepository _fingerprintRepository;
        private readonly ILogger<ExchangePublicTokenCommandHandler> _logger;

        public ExchangePublicTokenCommandHandler(
            IPlaidService plaidService,
            IPlaidAccountFingerprintRepository fingerprintRepository,
            ILogger<ExchangePublicTokenCommandHandler> logger)
        {
            _plaidService = plaidService;
            _fingerprintRepository = fingerprintRepository;
            _logger = logger;
        }

        public async Task<LinkSuccessResult> Handle(ExchangePublicTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check for duplicate if we have metadata with all required properties
                if (request.Metadata != null &&
                    request.Metadata.Accounts != null &&
                    !string.IsNullOrEmpty(request.Metadata.InstitutionId) &&
                    !string.IsNullOrEmpty(request.Metadata.InstitutionName))
                {
                    var userId = UserId.Create(request.UserId);
                    var accountDetails = request.Metadata.Accounts
                        .Where(a => a != null && !string.IsNullOrEmpty(a.Name) && !string.IsNullOrEmpty(a.Mask))
                        .Select(a => (a.Name, a.Mask))
                        .ToList();

                    // Only check for duplicates if we have account details
                    if (accountDetails.Any())
                    {
                        try
                        {
                            // Check if this is a duplicate
                            var existingFingerprint = await _fingerprintRepository.GetByInstitutionAndAccountsAsync(
                                userId,
                                request.Metadata.InstitutionId,
                                accountDetails);

                            if (existingFingerprint != null)
                            {
                                _logger.LogInformation(
                                    "Detected duplicate Item attempt for user {UserId} at institution {Institution}",
                                    request.UserId,
                                    request.Metadata.InstitutionName);

                                // Return the existing access token with IsDuplicate flag set to true
                                return new LinkSuccessResult(
                                    Success: true,
                                    existingFingerprint.AccessToken,
                                    existingFingerprint.ItemId,
                                    IsDuplicate: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error checking for duplicate items, continuing with normal flow");
                        }
                    }
                }

                // If no duplicate or no metadata to check, proceed with token exchange
                _logger.LogInformation("Exchanging public token for user {UserId}", request.UserId);
                var tokenResponse = await _plaidService.ExchangePublicTokenAsync(request.PublicToken, cancellationToken);

                // Store fingerprint if we have metadata
                if (request.Metadata != null &&
                    request.Metadata.Accounts != null &&
                    request.Metadata.Accounts.Any() &&
                    !string.IsNullOrEmpty(request.Metadata.InstitutionId) &&
                    !string.IsNullOrEmpty(request.Metadata.InstitutionName))
                {
                    try
                    {
                        var userId = UserId.Create(request.UserId);
                        var validAccounts = request.Metadata.Accounts
                            .Where(a => a != null && !string.IsNullOrEmpty(a.Mask) && !string.IsNullOrEmpty(a.Id))
                            .ToList();

                        if (validAccounts.Any())
                        {
                            var maskedAccountNumbers = string.Join("|", validAccounts.Select(a => a.Mask));

                            //InstitutionId for fingerprint generation to ensure consistency with the duplicate check
                            var fingerprint = PlaidAccountFingerprint.GenerateFingerprint(
                                request.Metadata.InstitutionId, // Use InstitutionId
                                maskedAccountNumbers,
                                request.UserId.ToString());

                            var accountIds = validAccounts.Select(a => a.Id).ToList();

                            var newFingerprint = new PlaidAccountFingerprint(
                                userId,
                                tokenResponse.AccessToken,
                                tokenResponse.ItemId,
                                fingerprint,
                                accountIds,
                                maskedAccountNumbers,
                                request.Metadata.InstitutionName);

                            await _fingerprintRepository.SaveFingerprintAsync(newFingerprint);
                            _logger.LogInformation("Saved account fingerprint for user {UserId}", request.UserId);
                        }
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex, "Error saving fingerprint but token exchange was successful");
                    }
                }

                return new LinkSuccessResult(
                    Success: true,
                    tokenResponse.AccessToken,
                    tokenResponse.ItemId,
                    IsDuplicate: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging public token for user {UserId}", request.UserId);
                throw;
            }
        }
    }
}