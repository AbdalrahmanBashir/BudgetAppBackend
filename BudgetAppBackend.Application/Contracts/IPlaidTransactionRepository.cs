using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.PlaidTransactionAggregate;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Application.Contracts
{
    public interface IPlaidTransactionRepository
    {
      
        Task AddTransactionsAsync(IEnumerable<PlaidTransaction> transactions);   // complete
        Task UpdateTransactionsAsync(IEnumerable<PlaidTransaction> transactions);  // complete
        Task MarkTransactionsAsRemovedAsync(IEnumerable<string> plaidTransactionIds); // complete
        Task<decimal> GetTotalIncomeForMonthAsync(UserId userId, DateTime currentDate, CancellationToken cancellationToken); // complete
        Task<decimal> GetTotalExpensesForMonthAsync(UserId userId, DateTime currentDate, CancellationToken cancellationToken);  // complete

        Task<decimal> GetTotalIncomeDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken); // complete
        Task<decimal> GetTotalExpensesDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken); // complete
        Task<IEnumerable<DetailedDailyCashFlowDto>> GetDetailedDailyCashFlowAsync(UserId userId,DateTime monthStartDate,CancellationToken cancellationToken); // complete
        Task<List<MonthlyCategoryTotalDto>> GetCategoryTotalsForLastFourMonthsAsync(string categoryName, UserId userId, CancellationToken cancellationToken);   // today
        Task<IQueryable<TransactionDto>> GetUserTransactionsQueryAsync(UserId userId, CancellationToken cancellationToken);  // complete
        Task<List<TransactionDto>> GetRecentTransactionsByUserAsync(UserId userId, CancellationToken cancellationToken);   // complete
        Task<PlaidTransaction?> GetByPlaidIdAsync(string plaidTransactionId);   // complete
        Task<IEnumerable<TransactionDto>> GetTopFiveTransactionsByCategory(UserId userId, string categoryName, DateTime currentMoth, CancellationToken cancellationToken); // complete
        Task<IEnumerable<TransactionDto>> GetUserTransactionsByDateRangeAsync(UserId userId, DateTime startDate, bool onlyWithCategory, CancellationToken cancellationToken);
        Task<List<PlaidTransaction>> GetUserTransactionsAsync(UserId userId, DateTime startDate, DateTime endDate); // is not being used
        Task<List<PlaidTransaction>> GetTransactionsByUserAndCategoryAsync(UserId userId, string category, DateTime budgetCreatedDate, CancellationToken cancellationToken);  // note sure

        Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAndDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken); // complete

        //Ai
        Task<IEnumerable<TransactionDto>> GetThreeMonthTransactionsByUserIdAsync(UserId userId);
        Task<IEnumerable<TransactionDto>> GetAllUserTransactionsAsync(UserId userId, CancellationToken cancellationToken);
    }
}
