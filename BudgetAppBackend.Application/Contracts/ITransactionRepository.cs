using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.TransactionAggregate;
using BudgetAppBackend.Domain.TransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Application.Contracts
{
    public interface ITransactionRepository
    {
        //CRUD
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken);  // complete
        Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken); // complete
        Task DeleteAsync(Transaction transaction, CancellationToken cancellationToken);  // complete

        //Financel summary
        Task<decimal> GetTotalIncomeForMonthAsync(UserId userId, DateTime currentDate, CancellationToken cancellationToken); // complete
        Task<decimal> GetTotalExpensesForMonthAsync(UserId userId, DateTime currentDate, CancellationToken cancellationToken); // complete

        Task<decimal> GetTotalIncomeDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken); // complete
        Task<decimal> GetTotalExpensesDateRangeAsync(UserId userId, DateTime startDate, CancellationToken cancellationToken); // complete
        Task<Transaction?> GetByIdAsync(TransactionId transactionId, CancellationToken cancellationToken);
        Task<IQueryable<TransactionDto>> GetUserTransactionsQueryAsync(UserId userId, CancellationToken cancellationToken);  //complete
        Task<IEnumerable<TransactionDto>> GetUserTransactionsByDateRangeAsync(UserId userId, DateTime startDate, bool onlyWithCategory, CancellationToken cancellationToken);
        Task<IEnumerable<TransactionDto>> GetRecentTransactionsByUserAsync(UserId userId, CancellationToken cancellationToken); // complete

        Task<IEnumerable<TransactionDto>> GetTopFiveTransactionsByCategory(UserId userId, string categoryName, DateTime currentMoth, CancellationToken cancellationToken); // complete

        Task<IEnumerable<DetailedDailyCashFlowDto>> GetDetailedDailyCashFlowAsync(UserId userId, DateTime monthStartDate, CancellationToken cancellationToken); // today
        Task<List<MonthlyCategoryTotalDto>> GetCategoryTotalsForLastFourMonthsAsync(string categoryName, UserId userId, CancellationToken cancellationToken);   // today
        Task<IEnumerable<Transaction>> GetTransactionsByUserAndCategoryAsync(UserId userId, string category, DateTime budgetCreatedDate, CancellationToken cancellationToken);

        Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAndDateRangeAsync(UserId userId,  DateTime startDate, CancellationToken cancellationToken); // complete

        //AI
        Task<IEnumerable<TransactionDto>> GetThreeMonthTransactionsByUserIdAsync(UserId userId); // complete 
        Task<IEnumerable<TransactionDto>> GetAllUserTransactionsAsync(UserId userId, CancellationToken cancellationToken); // complete
    }
}
