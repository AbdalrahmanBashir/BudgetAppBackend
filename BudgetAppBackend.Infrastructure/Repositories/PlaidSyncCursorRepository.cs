using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Infrastructure.Repositories
{
    public class PlaidSyncCursorRepository : IPlaidSyncCursorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlaidSyncCursorRepository> _logger;

        public PlaidSyncCursorRepository(
            ApplicationDbContext context,
            ILogger<PlaidSyncCursorRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PlaidSyncCursor?> GetLastCursorAsync(UserId userId, string accessToken)
        {
            try
            {
                return await _context.PlaidSyncCursors
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.AccessToken == accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving cursor for user {UserId} and access token {AccessToken}",
                    userId.Id, accessToken);
                throw;
            }
        }

        public async Task<PlaidSyncCursor?> GetCursorByItemIdAsync(UserId userId, string itemId)
        {
            try
            {
                return await _context.PlaidSyncCursors
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ItemId == itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving cursor for user {UserId} and item ID {ItemId}",
                    userId.Id, itemId);
                throw;
            }
        }

        public async Task SaveCursorAsync(PlaidSyncCursor cursor)
        {
            try
            {
                var existing = await _context.PlaidSyncCursors
                    .FirstOrDefaultAsync(c =>
                        c.UserId == cursor.UserId &&
                        c.ItemId == cursor.ItemId);

                if (existing == null)
                {
                    await _context.PlaidSyncCursors.AddAsync(cursor);
                }
                else
                {
                    existing.UpdateAccessToken(cursor.AccessToken);
                    existing.UpdateCursor(cursor.Cursor);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error saving cursor for user {UserId} and access token {AccessToken}",
                    cursor.UserId.Id, cursor.AccessToken);
                throw;
            }
        }

        public async Task<List<PlaidSyncCursor>> GetUserCursorsAsync(UserId userId)
        {
            try
            {
                return await _context.PlaidSyncCursors
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.LastSynced)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving cursors for user {UserId}",
                    userId.Id
                    );
                throw;
            }
        }

        public async Task DeleteCursorAsync(UserId userId, string itemId)
        {
            try
            {
                var cursor = await _context.PlaidSyncCursors
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ItemId == itemId);

                if (cursor != null)
                {

                    _context.PlaidSyncCursors.Remove(cursor);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning(
                        "Attempted to delete non-existent cursor for user {UserId} and access token {AccessToken}",
                        userId.Id, itemId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting cursor for user {UserId} and access token {AccessToken}",
                    userId.Id, itemId);
                throw;
            }
        }
    }
}