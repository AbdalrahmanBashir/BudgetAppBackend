using BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
using MediatR;

namespace BudgetAppBackend.Application.Features.AI.GetSpendingForecast
{
    public class GetSpendingForecastQuery : IRequest<DailySpendingForecastResult>
    {
        public Guid userId { get; set; }
    }
}
