using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.GetTop5ExpensByBudget
{
    public class GetTopNExpensesByBudgetQueryHandler : IRequestHandler<GetTopNExpensesByBudgetQuery, List<TransactionDto>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _laidTransactionRepository;
        private readonly IBudgetRepository _budgetRepository;
        public GetTopNExpensesByBudgetQueryHandler(ITransactionRepository transactionRepository, 
            IBudgetRepository budgetRepository, 
            IPlaidTransactionRepository plaidTransactionRepository)
        {
            _transactionRepository = transactionRepository;
            _budgetRepository = budgetRepository;
            _laidTransactionRepository = plaidTransactionRepository;
        }
        public async Task<List<TransactionDto>> Handle(GetTopNExpensesByBudgetQuery request, CancellationToken cancellationToken)
        { 

            var userId = UserId.Create(request.GetTopFiveTransactionsRequestDto.UserId);
            var budget = await _budgetRepository.GetBudgetAsync(userId, request.GetTopFiveTransactionsRequestDto.CategoryName, cancellationToken);
            if (budget == null)
            {
                throw new Exception("Budget not found");
            }

            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);  

            var manualTransactions = await _transactionRepository.GetTopFiveTransactionsByCategory(userId, request.GetTopFiveTransactionsRequestDto.CategoryName, currentMonth, cancellationToken);
            var plaidTransactions = await _laidTransactionRepository.GetTopFiveTransactionsByCategory(userId, request.GetTopFiveTransactionsRequestDto.CategoryName, currentMonth, cancellationToken);

            var allTransactions = manualTransactions.Concat(plaidTransactions);
            // Sort transactions by amount in descending order
            allTransactions = allTransactions.OrderByDescending(t => t.Amount).Take(5).ToList();


            return (List<TransactionDto>)allTransactions;

        }
    }
    
}
