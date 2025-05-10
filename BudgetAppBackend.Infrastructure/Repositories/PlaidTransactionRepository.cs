using System.Globalization;
using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.PlaidTransactionAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Infrastructure.Repositories
{
    public class PlaidTransactionRepository : IPlaidTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlaidTransactionRepository> _logger;

        public PlaidTransactionRepository(ApplicationDbContext context, ILogger<PlaidTransactionRepository> logger)
        {
            _context = context;
            _logger = logger;

        }

        public async Task AddTransactionsAsync(IEnumerable<PlaidTransaction> transactions)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var uniqueTransactions = transactions
                    .GroupBy(t => t.PlaidTransactionId)
                    .Select(g => g.First())
                    .ToList();

                var existingIds = await _context.PlaidTransactions
                    .Where(t => uniqueTransactions.Select(ut => ut.PlaidTransactionId).Contains(t.PlaidTransactionId))
                    .Select(t => t.PlaidTransactionId)
                    .ToListAsync();

                var newTransactions = uniqueTransactions
                    .Where(t => !existingIds.Contains(t.PlaidTransactionId))
                    .ToList();

                if (newTransactions.Any())
                {
                    await _context.PlaidTransactions.AddRangeAsync(newTransactions);
                }

                if (existingIds.Any())
                {
                    _logger.LogInformation(
                        "{Count} transactions already exist and will be skipped: {Ids}",
                        existingIds.Count,
                        string.Join(", ", existingIds));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PlaidTransaction?> GetByPlaidIdAsync(string plaidTransactionId)
        {
            return await _context.PlaidTransactions
                .FirstOrDefaultAsync(t => t.PlaidTransactionId == plaidTransactionId);
        }

        public async Task<List<TransactionDto>> GetRecentTransactionsByUserAsync(UserId userId, CancellationToken cancellationToken)
        {
            var transactions = await _context.PlaidTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .Select(t => new TransactionDto(
                    t.Id.Id,
                    t.Date,
                    t.Amount,
                    t.Name,
                    t.Categories))
                .ToListAsync(cancellationToken);

            return transactions;
        }

        public async Task<decimal> GetTotalExpensesForMonthAsync(UserId userId, DateTime currentDate, CancellationToken cancellationToken)
        {
            var transactions = await _context.PlaidTransactions
            .Where(t => t.UserId == userId &&
                        t.Date.Year == currentDate.Year &&
                        t.Date.Month == currentDate.Month)
            .ToListAsync(cancellationToken);

            return transactions
                        .Where(t => t.Amount < 0)
                        .Sum(t => Math.Abs(t.Amount));
        }

        public async Task<decimal> GetTotalIncomeForMonthAsync(UserId userId, DateTime currentDate, CancellationToken cancellationToken)
        {
            var transactions = await _context.PlaidTransactions
            .Where(t => t.UserId == userId &&
                        t.Date.Year == currentDate.Year &&
                        t.Date.Month == currentDate.Month)
            .ToListAsync(cancellationToken); // query happens here

            return transactions
             .Where(t => t.Amount > 0)
             .Sum(t => t.Amount);
        }

        public Task<IQueryable<TransactionDto>> GetUserTransactionsQueryAsync(UserId userId, CancellationToken cancellationToken)
        {
            var transactionsQuery = _context.PlaidTransactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionDto(
                    t.Id.Id,
                    t.Date,
                    t.Amount,
                    t.Name,
                    t.Categories
                ))
                .AsQueryable();

            return Task.FromResult(transactionsQuery);
        }

        public async Task<List<PlaidTransaction>> GetTransactionsByUserAndCategoryAsync(
             UserId userId,
             string category,
             DateTime budgetCreatedDate,
             CancellationToken cancellationToken)
        {
            var lowerCategory = category.ToLower();

            // Step 1: Fetch what EF Core can translate
            var transactions = await _context.PlaidTransactions
                .Where(t => t.UserId == userId &&
                            t.Date.Month == budgetCreatedDate.Month &&
                            t.Date.Year == budgetCreatedDate.Year)
                .ToListAsync(cancellationToken);

            // Step 2: Filter category in memory (case-insensitive)
            var filtered = transactions
                .Where(t => t.Categories.FirstOrDefault() != null &&
                            t.Categories.FirstOrDefault()!.ToLower().Contains(lowerCategory))
                .ToList();

            return filtered;
        }


        public async Task<List<PlaidTransaction>> GetUserTransactionsAsync(UserId userId, DateTime startDate, DateTime endDate)
        {
            return await _context.PlaidTransactions
                .Where(t => t.UserId == userId
                    && t.Date >= startDate
                    && t.Date <= endDate
                    && !t.IsRemoved)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task MarkTransactionsAsRemovedAsync(IEnumerable<string> transactionIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var uniqueIds = transactionIds.Distinct().ToList();
                var transactions = await _context.PlaidTransactions
                    .Where(t => uniqueIds.Contains(t.PlaidTransactionId))
                    .ToListAsync();

                if (!transactions.Any())
                {

                    return;
                }

                var missingIds = uniqueIds.Except(transactions.Select(t => t.PlaidTransactionId));
                if (missingIds.Any())
                {
                    
                }

                foreach (var plaidTransaction in transactions)
                {
                    plaidTransaction.MarkAsRemoved();
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task UpdateTransactionsAsync(IEnumerable<PlaidTransaction> transactions)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var uniqueTransactions = transactions
                    .GroupBy(t => t.PlaidTransactionId)
                    .Select(g => g.First())
                    .ToList();

                foreach (var plaidTransaction in uniqueTransactions)
                {
                    var existingTransaction = await _context.PlaidTransactions
                        .FirstOrDefaultAsync(t => t.PlaidTransactionId == plaidTransaction.PlaidTransactionId);

                    if (existingTransaction == null)
                    {
                     
                        continue;
                    }

                    _context.Update(plaidTransaction);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetThreeMonthTransactionsByUserIdAsync(UserId userId)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-90);
            var transactions = await _context.PlaidTransactions
                                    .Where(t => t.UserId == userId && t.Date >= cutoffDate)
                                    .ToListAsync();

            return transactions.Select(t => new TransactionDto(
                t.Id.Id,
                t.Date,
                t.Amount,
                t.Name,
                t.Categories
            ));
        }

        public async Task<IEnumerable<DetailedDailyCashFlowDto>> GetDetailedDailyCashFlowAsync(
             UserId userId,
             DateTime monthStartDate,
             CancellationToken cancellationToken)
        {
            var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

            var transactions = await _context.PlaidTransactions
                .Where(t => t.UserId == userId &&
                            t.Date >= monthStartDate &&
                            t.Date <= monthEndDate &&
                            !t.IsRemoved)
                .ToListAsync(cancellationToken);

            var grouped = transactions
                .GroupBy(t => DateTime.SpecifyKind(t.Date.Date, DateTimeKind.Utc))
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var income = g.Where(t => t.Amount > 0).Sum(t => t.Amount);
                        var expense = g.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
                        var net = income - expense;
                        return new { Income = income, Expense = expense, Net = net };
                    });

            var allDates = Enumerable.Range(0, (monthEndDate - monthStartDate).Days + 1)
                .Select(offset => DateTime.SpecifyKind(monthStartDate.AddDays(offset).Date, DateTimeKind.Utc));

            var results = new List<DetailedDailyCashFlowDto>();
            decimal runningTotal = 0;

            foreach (var date in allDates)
            {
                var income = grouped.TryGetValue(date, out var data) ? data.Income : 0;
                var expense = grouped.TryGetValue(date, out var data2) ? data2.Expense : 0;
                var net = income - expense;
                runningTotal += net;

                results.Add(new DetailedDailyCashFlowDto(
                    Date: date,
                    Income: income,
                    Expense: expense,
                    NetCashFlow: net,
                    CumulativeCashFlow: runningTotal
                ));
            }

            return results;
        }

        public async Task<IEnumerable<TransactionDto>> GetUserTransactionsByDateRangeAsync(UserId userId, DateTime startDate, bool onlyWithCategory, CancellationToken cancellationToken)
        {

            var query = await _context.PlaidTransactions
                .Where(t => t.UserId == userId)
                .Where(t => t.Date >= startDate)
                .ToListAsync(cancellationToken);

            if (onlyWithCategory)
            {
                query = query.Where(t => t.Categories.FirstOrDefault() != null).ToList();

            }

            return query.Select(t => new TransactionDto(
                 t.Id.Id,
                 t.Date,
                 t.Amount,
                 t.Name,
                 t.Categories)).ToList();
        }

        public async Task<List<MonthlyCategoryTotalDto>> GetCategoryTotalsForLastFourMonthsAsync(string categoryName, UserId userId, CancellationToken cancellationToken)
        {
            try
            {
                // Step 1: Determine the starting date (three months ago from now)
                var fromDate = DateTime.UtcNow.AddMonths(-3);

                // Step 2: Retrieve transactions filtered by user and date
                var transactions = await _context.PlaidTransactions
                    .Where(t => t.UserId == userId && t.Date >= fromDate && t.Amount < 0)
                    .ToListAsync();

                // Step 3: Filter by category in memory
                var filteredTransactions = transactions
                    .Where(t => t.Categories.Any(c => c.Contains(categoryName)))
                    .ToList();

                // Step 4: Group the filtered transactions by year and month
                var groupedTransactions = filteredTransactions
                    .GroupBy(t => new { t.Date.Year, t.Date.Month });

                // Step 5: Project each group into a MonthlyCategoryTotalDto
                var monthlyCategoryTotals = groupedTransactions
                    .Select(g => new MonthlyCategoryTotalDto
                    {
                        Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                        Total = g.Sum(t => t.Amount)
                    })
                    .OrderBy(dto => DateTime.ParseExact(dto.Month, "MMMM", CultureInfo.CurrentCulture))
                    .ToList();

                return monthlyCategoryTotals;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Log the exception (implementation depends on your logging framework)
                throw;
            }
        }


        public async Task<IEnumerable<TransactionDto>> GetTopFiveTransactionsByCategory(UserId userId, string categoryName, DateTime currentMonth, CancellationToken cancellationToken)
        {
            var transactions = await _context.PlaidTransactions
                .Where(t => t.UserId == userId &&
                            t.Date.Year == currentMonth.Year &&
                            t.Date.Month == currentMonth.Month &&
                            t.Amount < 0)
                .OrderBy(t => t.Amount) // Order by ascending to get the most negative amounts  
                .Select(t => new TransactionDto(
                    t.Id.Id,
                    t.Date,
                    t.Amount,
                    t.Name,
                    t.Categories))
                .ToListAsync(cancellationToken);

            // Filter transactions by category in memory
            transactions = transactions
                .Where(t => t.Categories.Any(c => c.Contains(categoryName)))
                .Take(5)
                .ToList();

            return transactions;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAndDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken)
        {
            var transactions = await _context.PlaidTransactions
                .Where(t => t.UserId == userId &&
                            t.Date >= startDate)
                .ToListAsync(cancellationToken);
            // Filter transactions by category in memory
            var Totaltransactions = transactions
                .Select(t => new TransactionDto(
                    t.Id.Id,
                    t.Date,
                    t.Amount,
                    t.Name,
                    t.Categories))
                .ToList();

            return Totaltransactions;
        }

        public async Task<decimal> GetTotalIncomeDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken)
        {
            var transactions = await _context.PlaidTransactions
            .Where(t => t.UserId == userId &&
                        t.Date >= startDate)
            .ToListAsync(cancellationToken); // query happens here

            return transactions
             .Where(t => t.Amount > 0)
             .Sum(t => t.Amount);

        }

        public async Task<decimal> GetTotalExpensesDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken)
        {
            var transactions = await _context.PlaidTransactions
                .Where(t => t.UserId == userId &&
                            t.Date >= startDate)
                .ToListAsync(cancellationToken);

                 return transactions
                 .Where(t => t.Amount < 0)
                 .Sum(t => Math.Abs(t.Amount));
        }

        public async Task<IEnumerable<TransactionDto>> GetAllUserTransactionsAsync(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.PlaidTransactions
                .Where(t => t.UserId == userId)
                .Select(t => new TransactionDto(
                    t.Id.Id,
                    t.Date,
                    t.Amount,
                    t.Name,
                    t.Categories))
                .ToListAsync(cancellationToken);
        }
    }
}