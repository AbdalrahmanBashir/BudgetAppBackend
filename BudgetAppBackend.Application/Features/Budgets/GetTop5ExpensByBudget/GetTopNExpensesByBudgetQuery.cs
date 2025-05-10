using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.GetTop5ExpensByBudget
{
    public class GetTopNExpensesByBudgetQuery : IRequest<List<TransactionDto>>
    {
        public GetTopFiveTransactionsRequest GetTopFiveTransactionsRequestDto { get; set; }
    }

}
