using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.BudgetAggregate;
using BudgetAppBackend.Domain.BudgetAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BudgetAppBackend.Infrastructure.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public BudgetRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Budget budget, CancellationToken cancellationToken)
        {
            await _dbContext.Budgets.AddAsync(budget, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Budget budget, CancellationToken cancellationToken)
        {
            _dbContext.Budgets.Remove(budget);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Budget>> GetActiveBudgetsAsync(UserId userId ,CancellationToken cancellationToken)
        {
            return await _dbContext.Budgets
                .Where(b => b.IsActive && b.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        
        public async Task<Budget?> GetByIdAsync(BudgetId budgetId, CancellationToken cancellationToken)
        {
            return await _dbContext.Budgets
                .FirstOrDefaultAsync(b => b.Id == budgetId, cancellationToken);
        }

        public async Task UpdateAsync(Budget budget, CancellationToken cancellationToken)
        {
            _dbContext.Budgets.Update(budget);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Budget>> GetAllBudgetsAsync()
        {
            return await _dbContext.Budgets.ToListAsync();
        }

        public async Task<IEnumerable<Budget>> GetBudgetsByUserIdAsync(UserId userId)
        {
            return await _dbContext.Budgets
                                 .Where(b => b.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<Budget?> GetByCategoryAsync(string categoryName, UserId userId, DateTime date, CancellationToken cancellationToken)
        {
            return await _dbContext.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.CreatedDate.Year == date.Year &&
                    b.CreatedDate.Month == date.Month &&
                    b.Category == categoryName);
        }

        public async Task<Budget?> GetBudgetAsync(UserId userId, string categoryName, CancellationToken cancellationToken)
        {
            var budget = await _dbContext.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Category == categoryName, cancellationToken);
            if (budget == null)
            {
                return null;
            }
            return budget;
        }
    }
}
