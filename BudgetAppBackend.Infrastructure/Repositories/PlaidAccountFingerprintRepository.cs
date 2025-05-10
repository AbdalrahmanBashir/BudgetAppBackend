using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Infrastructure.Repositories
{
    public class PlaidAccountFingerprintRepository : IPlaidAccountFingerprintRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlaidAccountFingerprintRepository> _logger;

        public PlaidAccountFingerprintRepository(
            ApplicationDbContext context,
            ILogger<PlaidAccountFingerprintRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task DeleteFingerprintAsync(UserId userId, string itemId)
        {
            try
            {
                var fingerprint = await _context.PlaidAccountFingerprints
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ItemId == itemId);

                if (fingerprint != null)
                {
                    _context.PlaidAccountFingerprints.Remove(fingerprint);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning(
                        "Attempted to delete non-existent fingerprint for user {UserId} and item ID {ItemId}",
                        userId.Id, itemId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting fingerprint for user {UserId} and item ID {ItemId}",
                    userId.Id, itemId);
                throw;
            }
        }

        public async Task<PlaidAccountFingerprint?> GetByInstitutionAndAccountsAsync(UserId userId, string institutionId, List<(string name, string mask)> accounts)
        {
            try
            {

                var maskedAccountNumbers = string.Join("|", accounts.Select(a => a.mask));
                var fingerprint = PlaidAccountFingerprint.GenerateFingerprint(
                    institutionId,
                    maskedAccountNumbers,
                    userId.Id.ToString());

                var exactMatch = await _context.PlaidAccountFingerprints
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.Fingerprint == fingerprint);

                if (exactMatch != null)
                {
                    return exactMatch;
                }

                var candidatesByInstitution = await _context.PlaidAccountFingerprints
                    .Where(f => f.UserId == userId && f.InstitutionName == institutionId)
                    .ToListAsync();

                foreach (var candidate in candidatesByInstitution)
                {
                    var candidateAccountMasks = candidate.MaskedAccountNumbers.Split('|');
                    var currentAccountMasks = accounts.Select(a => a.mask).ToList();

                    var matchCount = candidateAccountMasks.Count(mask => currentAccountMasks.Contains(mask));
                    var requiredMatchCount = Math.Min(candidateAccountMasks.Length, currentAccountMasks.Count) / 2;

                    if (matchCount >= requiredMatchCount)
                    {
                        _logger.LogInformation(
                            "Found partial match for user {UserId} at institution {Institution}",
                            userId.Id,
                            institutionId);
                        return candidate;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error finding fingerprint for user {UserId} at institution {Institution}",
                    userId.Id, institutionId);
                throw;
            }
        }

        public async Task<PlaidAccountFingerprint?> GetByItemIdAsync(UserId userId, string itemId)
        {
            try
            {
                return await _context.PlaidAccountFingerprints
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ItemId == itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error finding fingerprint for user {UserId} with item ID {ItemId}",
                    userId.Id, itemId);
                throw;
            }
        }

        public async Task<List<PlaidAccountFingerprint>> GetByUserIdAsync(UserId userId)
        {
            try
            {
                return await _context.PlaidAccountFingerprints
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.LastUpdated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving fingerprints for user {UserId}",
                    userId.Id);
                throw;
            }
        }

        public async Task SaveFingerprintAsync(PlaidAccountFingerprint fingerprint)
        {
            try
            {
                await _context.PlaidAccountFingerprints.AddAsync(fingerprint);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error saving fingerprint for user {UserId} and item ID {ItemId}",
                    fingerprint.UserId.Id, fingerprint.ItemId);
                throw;
            }
        }

        public async Task UpdateFingerprintAsync(PlaidAccountFingerprint fingerprint)
        {
            try
            {
                _context.PlaidAccountFingerprints.Update(fingerprint);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating fingerprint for user {UserId} and item ID {ItemId}",
                    fingerprint.UserId.Id, fingerprint.ItemId);
                throw;
            }
        }
    }
}
