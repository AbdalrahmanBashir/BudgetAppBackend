using BudgetAppBackend.Application.Features.AI.GetQuarterlyTransactionAnalysis;
using BudgetAppBackend.Application.Features.AI.GetSpendingForecast;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAppBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiAnalysisController : ControllerBase
    {
        private readonly ISender _sender;
            

        public AiAnalysisController(ISender sender)
        {
            _sender = sender; 
        }

        [HttpGet("QuarterlyTransactionAnalysis/{userId}")]
        public async Task<IActionResult> GetQuarterlyTransactionAnalysis(Guid userId)
        {

          
            var result = await _sender.Send(new GetQuarterlyTransactionAnalysisQuery
            {
                UserId = userId
            });
            return Ok(result);
        }

        [HttpGet("ForecastSpendingTrends/{UserId}")]
        public async Task<IActionResult> GetForecastSpendingTrends(Guid UserId)
        {


            var result = await _sender.Send(new GetSpendingForecastQuery
            {
                userId = UserId
            });
            return Ok(result);
        }
    }
}
