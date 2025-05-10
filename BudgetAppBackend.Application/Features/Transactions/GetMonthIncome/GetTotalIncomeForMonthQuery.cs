using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetMonthIncome
{
    public class GetTotalIncomeForMonthQuery : IRequest<decimal>
    {
        public Guid UserId { get; set; }
    }
}
