using BudgetAppBackend.Domain.BudgetAggregate.ValueObjects;
using BudgetAppBackend.Domain.BudgetAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Application.Contracts
{
    public interface IBudgetRepository
    {
        Task AddAsync(Budget budget, CancellationToken cancellationToken);//
        Task<Budget?> GetByIdAsync(BudgetId budgetId, CancellationToken cancellationToken);  //
        Task<Budget?> GetBudgetAsync(UserId userId, string categoryName, CancellationToken cancellationToken);  //
        Task<List<Budget>> GetActiveBudgetsAsync(UserId userId, CancellationToken cancellationToken); //
        Task<Budget?> GetByCategoryAsync(string categoryName,UserId userId, DateTime date, CancellationToken cancellationToken);
        Task UpdateAsync(Budget budget, CancellationToken cancellationToken); //
        Task DeleteAsync(Budget budget, CancellationToken cancellationToken);
        Task<List<Budget>> GetAllBudgetsAsync();
        Task<IEnumerable<Budget>> GetBudgetsByUserIdAsync(UserId userId);
    }
}
