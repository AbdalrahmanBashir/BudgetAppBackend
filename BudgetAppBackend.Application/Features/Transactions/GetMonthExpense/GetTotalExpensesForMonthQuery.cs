using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetMonthExpense
{
    public class GetTotalExpensesForMonthQuery : IRequest<decimal>
    {
        public Guid UserId { get; set; }
    }
}
