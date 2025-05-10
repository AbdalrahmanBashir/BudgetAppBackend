using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.Features.Budgets.CreateBudget;
using BudgetAppBackend.Application.Features.Budgets.DeleteBudget;
using BudgetAppBackend.Application.Features.Budgets.GetBudgetsByUser;
using BudgetAppBackend.Application.Features.Budgets.GetCategoryTotalsForLastFourMonths;
using BudgetAppBackend.Application.Features.Budgets.GetTop5ExpensByBudget;
using BudgetAppBackend.Application.Features.Budgets.UpdateBudget;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAppBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetController : ControllerBase
    {
        private readonly ISender _sender;

        public BudgetController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto createBudgetDto)
        {
            var result = await _sender.Send(new CreateBudgetCommand
            {
                CreateBudgetDto = createBudgetDto,
            });
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateBudget([FromBody] UpdateBudgetDto updateBudgetDto)
        {

            var result = await _sender.Send(new UpdateBudgetCommand
            {
                UpdateBudgetDto = updateBudgetDto
            });
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBudget(DeleteBudgetDto deleteBudgetDto)
        {
            var result = await _sender.Send(new DeleteBudgetCommand { DeleteBudgetDto = deleteBudgetDto });
            return Ok(result);
        }

        [HttpGet("list-of-budgets")]
        public async Task<IActionResult> GetBudgetsByUser(Guid userId)
        {

            var result = await _sender.Send(new GetBudgetsByUserQuery { UserId = userId });
            return Ok(result);
        }

        [HttpGet("last-four-mothns-total")]
        public async Task<IActionResult> GetLastFourMonthsTotal(Guid userId, string categoryName)
        {
            var getTotalBudgetForLastFourMonths = new GetTotalBudgetForLastFourMonths
            {
                UserId = userId,
                Category = categoryName
            };

            var result = await _sender.Send(new GetCategoryTotalsForLastFourMonthsQuery
            {
                GetTotalBudgetForLastFourMonthsDto = getTotalBudgetForLastFourMonths
            });

            return Ok(result);
        }

        [HttpGet("top-five-current-month-transaction-by-budget")]
        public async Task<IActionResult> GetTopFiveTransactionForBudget(Guid userId, string categoryName)
        {
            var getTopFiveTransactionsRequest = new GetTopFiveTransactionsRequest
            {
                UserId = userId,
                CategoryName = categoryName
            };

            var result = await _sender.Send(new GetTopNExpensesByBudgetQuery
            {
                GetTopFiveTransactionsRequestDto = getTopFiveTransactionsRequest
            });

            return Ok(result);
        }
    }
       
}
