using BudgetAppBackend.Application.Models.PlaidModels;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using BudgetAppBackend.Domain.PlaidTransactionAggregate;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;

namespace BudgetAppBackend.Application.Features.Plaid.SyncTransactions
{
    public class SyncTransactionsCommandHandler : IRequestHandler<SyncTransactionsCommand, TransactionsSyncResponse>
    {
        private readonly IPlaidService _plaidService;
        private readonly IPlaidTransactionRepository _transactionRepository;
        private readonly IPlaidSyncCursorRepository _cursorRepository;
        private readonly IPlaidAccountFingerprintRepository _fingerprintRepository;
        private readonly ILogger<SyncTransactionsCommandHandler> _logger;

        public SyncTransactionsCommandHandler(
            IPlaidService plaidService,
            IPlaidTransactionRepository transactionRepository,
            IPlaidSyncCursorRepository cursorRepository,
            IPlaidAccountFingerprintRepository fingerprintRepository,
            ILogger<SyncTransactionsCommandHandler> logger)
        {
            _plaidService = plaidService ?? throw new ArgumentNullException(nameof(plaidService));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _cursorRepository = cursorRepository ?? throw new ArgumentNullException(nameof(cursorRepository));
            _fingerprintRepository = fingerprintRepository ?? throw new ArgumentNullException(nameof(fingerprintRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TransactionsSyncResponse> Handle(
            SyncTransactionsCommand request,
            CancellationToken cancellationToken)
        {
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (request.userId == null)
            {
                _logger.LogError("User ID cannot be null or empty");
                throw new ArgumentException("User ID is required", nameof(request.userId));
            }
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (string.IsNullOrEmpty(request.AccessToken))
            {
                _logger.LogError("Access token cannot be null or empty");
                throw new ArgumentException("Access token is required", nameof(request.AccessToken));
            }

            _logger.LogInformation("Beginning transaction sync for user {UserId}", request.userId);

            try
            {
                var userId = UserId.Create(request.userId);
                string? itemId = null;
                string? cursor = request.Cursor;

                // If no cursor was provided, try to find the last cursor
                if (cursor == null)
                {
                    try
                    {
                        var savedCursor = await _cursorRepository.GetLastCursorAsync(userId, request.AccessToken);
                        cursor = savedCursor?.Cursor;

                        // If we found a cursor, also get the itemId from it
                        if (savedCursor != null)
                        {
                            itemId = savedCursor.ItemId;
                            _logger.LogInformation("Found existing cursor for userId {UserId}: {Cursor}",
                                request.userId, cursor?.Substring(0, Math.Min(cursor.Length, 10)) + "...");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Don't fail the whole operation if we can't get the cursor
                        _logger.LogWarning(ex, "Error retrieving last cursor for user {UserId}. " +
                            "Will proceed with full sync.", request.userId);
                    }
                }

                // Get transactions from Plaid with the cursor we found (or null for initial sync)
                _logger.LogInformation("Calling Plaid API to sync transactions for user {UserId} with cursor {Cursor}",
                    request.userId, cursor ?? "null");

                var syncResponse = await _plaidService.SyncTransactionsAsync(
                    request.userId,
                    request.AccessToken,
                    cursor,
                    request.Count,
                    cancellationToken);

                if (syncResponse == null)
                {
                    _logger.LogError("Received null response from Plaid service");
                    throw new InvalidOperationException("Plaid service returned null response");
                }

                // Get item ID from sync response
                itemId = syncResponse.ItemId ?? itemId;

                _logger.LogInformation("Received sync response for user {UserId}. " +
                    "Added: {AddedCount}, Modified: {ModifiedCount}, Removed: {RemovedCount}, HasMore: {HasMore}",
                    request.userId,
                    syncResponse.Added?.Count ?? 0,
                    syncResponse.Modified?.Count ?? 0,
                    syncResponse.Removed?.Count ?? 0,
                    syncResponse.HasMore);

                // Process added transactions
                if (syncResponse.Added?.Any() == true)
                {
                    try
                    {
                        var addedEntities = syncResponse.Added
                            .Where(t => t != null && !string.IsNullOrEmpty(t.PlaidTransactionId))
                            .OrderByDescending(t => t.Date)
                            .Select(CreateDomainEntity)
                            .ToList();

                        await _transactionRepository.AddTransactionsAsync(addedEntities);
                        _logger.LogInformation("Added {Count} new transactions for user {UserId}",
                            addedEntities.Count, request.userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing added transactions for user {UserId}", request.userId);
                        // Don't rethrow - we want to continue with modified and removed transactions
                    }
                }

                // Process modified transactions
                if (syncResponse.Modified?.Any() == true)
                {
                    try
                    {
                        var validModifiedDtos = syncResponse.Modified
                            .Where(t => t != null && !string.IsNullOrEmpty(t.PlaidTransactionId))
                            .ToList();

                        var modifiedEntities = new List<PlaidTransaction>();

                        foreach (var dto in validModifiedDtos)
                        {
                            try
                            {
                                var existingTransaction = await _transactionRepository.GetByPlaidIdAsync(dto.PlaidTransactionId);
                                if (existingTransaction != null)
                                {
                                    existingTransaction.Update(
                                        dto.Amount,
                                        dto.Name,
                                        dto.Date,
                                        dto.Categories,
                                        dto.CategoryId,
                                        dto.MerchantName);

                                    modifiedEntities.Add(existingTransaction);
                                }
                                else
                                {
                                    // If the transaction doesn't exist, treat it as a new one
                                    _logger.LogWarning("Transaction to modify not found, creating new: {Id}",
                                        dto.PlaidTransactionId);

                                    var newTransaction = CreateDomainEntity(dto);
                                    await _transactionRepository.AddTransactionsAsync(new[] { newTransaction });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing modified transaction {Id}", dto.PlaidTransactionId);
                                // Continue with other transactions
                            }
                        }

                        if (modifiedEntities.Any())
                        {
                            await _transactionRepository.UpdateTransactionsAsync(modifiedEntities.ToArray());
                            _logger.LogInformation("Updated {Count} transactions for user {UserId}",
                                modifiedEntities.Count, request.userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing modified transactions for user {UserId}", request.userId);
                        // Don't rethrow - we want to continue with removed transactions
                    }
                }

                // Process removed transactions
                if (syncResponse.Removed?.Any() == true)
                {
                    try
                    {
                        var removedIds = syncResponse.Removed
                            .Where(t => t != null && !string.IsNullOrEmpty(t.TransactionId))
                            .Select(t => t.TransactionId)
                            .ToList();

                        if (removedIds.Any())
                        {
                            await _transactionRepository.MarkTransactionsAsRemovedAsync(removedIds);
                            _logger.LogInformation("Marked {Count} transactions as removed for user {UserId}",
                                removedIds.Count, request.userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing removed transactions for user {UserId}", request.userId);
                        // Don't rethrow - we want to continue with cursor update
                    }
                }

                // Only update the fingerprint if we have an itemId
                if (!string.IsNullOrEmpty(itemId))
                {
                    try
                    {
                        // Look up fingerprint by ItemId
                        var fingerprint = await _fingerprintRepository.GetByItemIdAsync(userId, itemId);
                        if (fingerprint != null)
                        {
                            // Update access token if it changed
                            if (fingerprint.AccessToken != request.AccessToken)
                            {
                                fingerprint.UpdateTokenAndItemId(request.AccessToken, itemId);
                                await _fingerprintRepository.UpdateFingerprintAsync(fingerprint);
                                _logger.LogInformation("Updated access token for Item ID {ItemId}", itemId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating fingerprint for user {UserId}, Item ID {ItemId}",
                            request.userId, itemId);
                        // Don't rethrow - this is not critical for transaction sync
                    }
                }
                else
                {
                    _logger.LogWarning("No Item ID available after sync for user {UserId}. Using access token only.",
                        request.userId);
                }

                // Update the sync cursor if we have a next cursor
                if (!string.IsNullOrEmpty(syncResponse.NextCursor))
                {
                    try
                    {
                        var newCursor = PlaidSyncCursor.Create(
                            userId,
                            request.AccessToken,
                            itemId ?? "unknown",  // Fallback if somehow we don't have an ItemId
                            syncResponse.NextCursor);

                        await _cursorRepository.SaveCursorAsync(newCursor);
                        _logger.LogInformation("Updated sync cursor for user {UserId}, Item ID {ItemId}",
                            request.userId, itemId ?? "unknown");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving cursor for user {UserId}", request.userId);
                        // Don't rethrow - returning the transactions is more important
                    }
                }

                _logger.LogInformation("Completed transaction sync for user {UserId}", request.userId);
                return syncResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error syncing transactions for user {UserId}", request.userId);

                // Check for inner exceptions and log them too
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    _logger.LogError("Inner exception: {Type} - {Message}",
                        innerEx.GetType().Name, innerEx.Message);
                    innerEx = innerEx.InnerException;
                }

                throw; // Rethrow to let the controller handle it
            }
        }

        private static PlaidTransaction CreateDomainEntity(PlaidTransactionDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (string.IsNullOrEmpty(dto.PlaidTransactionId))
            {
                throw new ArgumentException("Transaction ID cannot be null or empty", nameof(dto.PlaidTransactionId));
            }


            if (dto.userId == null)
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(dto.userId));
            }


            return PlaidTransaction.Create(
                UserId.Create(dto.userId),
                dto.PlaidTransactionId,
                dto.AccountId ?? string.Empty,
                dto.Amount,
                dto.Name ?? string.Empty,
                dto.Date,
                dto.Categories ?? [],
                dto.CategoryId ?? string.Empty,
                dto.MerchantName ?? string.Empty);
        }
    }
}