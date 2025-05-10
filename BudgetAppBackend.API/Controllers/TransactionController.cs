using BudgetAppBackend.Application.DTOs.PagingsDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Application.Features.Transactions.CreateTransaction;
using BudgetAppBackend.Application.Features.Transactions.DeleteTransaction;
using BudgetAppBackend.Application.Features.Transactions.GetDetailedDailyCashFlow;
using BudgetAppBackend.Application.Features.Transactions.GetMonthExpense;
using BudgetAppBackend.Application.Features.Transactions.GetMonthIncome;
using BudgetAppBackend.Application.Features.Transactions.GetRecentTransactions;
using BudgetAppBackend.Application.Features.Transactions.GetSpendingPerCategory;
using BudgetAppBackend.Application.Features.Transactions.GetUserTransactionsWithPagination;
using BudgetAppBackend.Application.Features.Transactions.UpdateTransaction;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAppBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ISender _sender;
        public TransactionController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("user/{userId}/list-of-transactions")]
        public async Task<ActionResult<PagedResponse<TransactionDto>>> GetUserTransactions(
            Guid userId,
            [FromQuery] PagingDTO pagingDto,
            [FromQuery] TransactionFilterDto filterDto,
            CancellationToken cancellationToken)
        {
            var transactions = await _sender.Send(new GetUserTransactionsWithPaginationQuery
            {
                UserId = userId,
                Paging = pagingDto,
                Filter = filterDto
            }, cancellationToken);

            return Ok(transactions);
        }

        [HttpGet("user/{userId}/recent-transactions")]
        public async Task<ActionResult<List<TransactionDto>>> GetRecentTransactions(Guid userId, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetRecentTransactionsQuery
            {
                UserId = userId
            }, cancellationToken);
            return Ok(response);
        }

        [HttpGet("user/{userId}/monthly-expenses")]
        public async Task<ActionResult<decimal>> GetTotalExpensesForMonth(Guid userId, CancellationToken cancellationToken)
        {
            var totalExpenses = await _sender.Send(new GetTotalExpensesForMonthQuery {UserId=userId }, cancellationToken);
            return Ok(totalExpenses);
        }

        [HttpGet("user/{userId}/monthly-income")]
        public async Task<ActionResult<decimal>> GetTotalIncomeForMonth(Guid userId, CancellationToken cancellationToken)
        {
            var totalIncome = await _sender.Send(new GetTotalIncomeForMonthQuery { UserId=userId }, cancellationToken);
            return Ok(totalIncome);
        }

        [HttpPost("create")]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(
            [FromBody] CreateTransactionDto transactionDto, CancellationToken cancellationToken)
        {
            var command = new CreateTransactionCommand { createTransactionDto = transactionDto };
            var response = await _sender.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetUserTransactions), new { userId = transactionDto.UserId }, response);
        }



        [HttpPut("update/{transactionId}")]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionDto transactionDto, CancellationToken cancellationToken)
        {
            var command = new UpdateTransactionCommand
            {
                UpdateTransactionDto = transactionDto
            };

            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("delete/{transactionId}")]
        public async Task<IActionResult> DeleteTransaction([FromBody] DeleteTransactionDto transactionDto, CancellationToken cancellationToken)
        {
            var command = new DeleteTransactionCommand { DeleteTransactionDto = transactionDto };

            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpGet("users/{userId}/month-cashflow")]
        public async Task<IActionResult> GetCashFlow(Guid userId)
        {
            var result = await _sender.Send(new GetDetailedDailyCashFlowQuery(
                userId
            ));

            return Ok(result);
        }

        [HttpGet("users/{userId}/spending-per-category")]
        public async Task<IActionResult> GetSpendingPerCategory(Guid userId)
        {
            var result = await _sender.Send(new GetSpendingPerCategoryQuery(
                userId
            ));

            return Ok(result);
        }
    }
}