using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.AI.GetSpendingForecast
{
    /// <summary>
    /// Handles the query to forecast daily spending using the last 3 months of Plaid transactions.
    /// </summary>
    public class GetSpendingForecastQueryHandler : IRequestHandler<GetSpendingForecastQuery, DailySpendingForecastResult>
    {
        private readonly IDailySpendingForecaster _dailySpendingForecaster;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _plaidTransactionRepository;

        public GetSpendingForecastQueryHandler(
            IDailySpendingForecaster dailySpendingForecaster,
            ITransactionRepository transactionRepository,
            IPlaidTransactionRepository plaidTransactionRepository)
        {
            _dailySpendingForecaster = dailySpendingForecaster;
            _transactionRepository = transactionRepository;
            _plaidTransactionRepository = plaidTransactionRepository;
        }

        public async Task<DailySpendingForecastResult> Handle(GetSpendingForecastQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.userId);

            // Get last 3 months of Plaid transactions
            var plaidTransactions = (await _plaidTransactionRepository.GetThreeMonthTransactionsByUserIdAsync(userId))
                ?? Enumerable.Empty<TransactionDto>();

            if (!plaidTransactions.Any())
                throw new KeyNotFoundException("No transactions to analyze");

            // Group only negative amounts (expenses) by date
            var dailyExpenses = plaidTransactions
               .Where(t => t.Amount < 0)
               .GroupBy(t => t.TransactionDate.Date)
               .OrderBy(g => g.Key)
               .Select(g => new
               {
                   Date = g.Key,
                   Total = Math.Abs(g.Sum(t => t.Amount)) // use absolute to make all values positive
               })
               .ToList();

            if (!dailyExpenses.Any())
                throw new InvalidOperationException("No expenses found in the transaction history");

            float[] dailyAmounts = dailyExpenses.Select(g => (float)g.Total).ToArray();
            var lastKnownDate = dailyExpenses.Last().Date;

            var forecast = await _dailySpendingForecaster.ForecastAsync(dailyAmounts, lastKnownDate, 30);

            return forecast;
        }
    }
}
