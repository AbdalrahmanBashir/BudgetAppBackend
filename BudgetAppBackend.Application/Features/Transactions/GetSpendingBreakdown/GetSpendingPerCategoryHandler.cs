using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetSpendingPerCategory
{
    public class GetSpendingBreakdownQueryHandler : IRequestHandler<GetSpendingPerCategoryQuery, IEnumerable<CategoryTotalDto>>
    {

        private readonly ITransactionRepository _transactionReadRepository;
        private readonly IPlaidTransactionRepository _plaidTransactionRepository;

        public GetSpendingBreakdownQueryHandler(
            ITransactionRepository transactionReadRepository,
            IPlaidTransactionRepository plaidTransactionRepository)
        {
            _transactionReadRepository = transactionReadRepository;
            _plaidTransactionRepository = plaidTransactionRepository;
        }
        public async Task<IEnumerable<CategoryTotalDto>> Handle(GetSpendingPerCategoryQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.UserId);

            var monthStartDate = DateTime.UtcNow.AddDays(-30);  // from today go back 30 days.

            var manualTransactions = await _transactionReadRepository.GetUserTransactionsByDateRangeAsync(userId, monthStartDate, true, cancellationToken);
            var plaidTransactions = await _plaidTransactionRepository.GetUserTransactionsByDateRangeAsync(userId, monthStartDate, true, cancellationToken);

            var allTransactions = manualTransactions.Concat(plaidTransactions)
                                         .OrderByDescending(t => t.TransactionDate)
                                         .AsQueryable();

            var spendingPerCategory = allTransactions
                        .Where (t => t.Amount < 0) // Whitelist expenses
                        .GroupBy(t => t.Categories.First()) // Group by the primary category
                        .Select(g => new CategoryTotalDto { Category = g.Key, TotalAmount = g.Sum(t => t.Amount) })
                        .ToList();

            return spendingPerCategory;
        }
    }
}