using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.GetCategoryTotalsForLastFourMonths
{
    public class GetCategoryTotalsForLastFourMonthsQuery : IRequest<List<MonthlyCategoryTotalDto>>
    {
        public GetTotalBudgetForLastFourMonths GetTotalBudgetForLastFourMonthsDto { get; set; } = default!;

    }
}
