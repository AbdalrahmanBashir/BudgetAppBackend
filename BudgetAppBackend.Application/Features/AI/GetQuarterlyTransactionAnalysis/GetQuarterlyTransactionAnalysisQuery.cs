using BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
using MediatR;

namespace BudgetAppBackend.Application.Features.AI.GetQuarterlyTransactionAnalysis
{
    public class GetQuarterlyTransactionAnalysisQuery : IRequest<QuarterlyTransactionAnalysis>
    {
        public Guid UserId { get; set; }
    }
}
