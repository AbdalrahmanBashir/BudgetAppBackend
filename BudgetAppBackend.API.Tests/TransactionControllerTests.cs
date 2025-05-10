using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetAppBackend.API.Controllers;
using BudgetAppBackend.Application.DTOs.PagingsDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Application.Features.Transactions.CreateTransaction;
using BudgetAppBackend.Application.Features.Transactions.DeleteTransaction;
using BudgetAppBackend.Application.Features.Transactions.GetDetailedDailyCashFlow;
using BudgetAppBackend.Application.Features.Transactions.GetRecentTransactions;
using BudgetAppBackend.Application.Features.Transactions.GetSpendingPerCategory;
using BudgetAppBackend.Application.Features.Transactions.GetUserTransactionsWithPagination;
using BudgetAppBackend.Application.Features.Transactions.UpdateTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using BudgetAppBackend.Application.Features.Transactions.GetMonthExpense;
using BudgetAppBackend.Application.Features.Transactions.GetMonthIncome;

namespace BudgetAppBackend.API.Tests
{
    public class TransactionControllerTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly TransactionController _controller;

        public TransactionControllerTests()
        {
            _mockSender = new Mock<ISender>();
            _controller = new TransactionController(_mockSender.Object);
        }

        [Fact]
        public async Task GetUserTransactions_ReturnsOk_WithPagedResponse()
        {
            var userId = Guid.NewGuid();
            var pagingDto = new PagingDTO();
            var filterDto = new TransactionFilterDto();

            var transactionList = new List<TransactionDto>
            {
                new TransactionDto(Guid.NewGuid(), DateTime.UtcNow, 50, "Note1", new List<string>()),
                new TransactionDto(Guid.NewGuid(), DateTime.UtcNow, 100, "Note2", new List<string>())
            };

            var expected = new PagedResponse<TransactionDto>(transactionList.AsQueryable(), pagingDto);

            _mockSender.Setup(s => s.Send(It.IsAny<GetUserTransactionsWithPaginationQuery>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expected);

            var result = await _controller.GetUserTransactions(userId, pagingDto, filterDto, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            (result.Result as OkObjectResult)!.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetRecentTransactions_ReturnsOk_WithTransactions()
        {
            var userId = Guid.NewGuid();
            var expected = new List<TransactionDto>
            {
                new TransactionDto(Guid.NewGuid(), DateTime.UtcNow, 20.5m, "Coffee", new List<string> { "Cafe" }),
                new TransactionDto(Guid.NewGuid(), DateTime.UtcNow, 80.0m, "Groceries", new List<string> { "Food" })
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetRecentTransactionsQuery>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expected);

            var result = await _controller.GetRecentTransactions(userId, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            (result.Result as OkObjectResult)!.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetTotalExpensesForMonth_ReturnsOk_WithDecimal()
        {
            var userId = Guid.NewGuid();
            decimal expected = 300.75M;

            _mockSender.Setup(s => s.Send(It.IsAny<GetTotalExpensesForMonthQuery>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expected);

            var result = await _controller.GetTotalExpensesForMonth(userId, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            (result.Result as OkObjectResult)!.Value.Should().Be(expected);
        }

        [Fact]
        public async Task GetTotalIncomeForMonth_ReturnsOk_WithDecimal()
        {
            var userId = Guid.NewGuid();
            decimal expected = 950.50M;

            _mockSender.Setup(s => s.Send(It.IsAny<GetTotalIncomeForMonthQuery>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expected);

            var result = await _controller.GetTotalIncomeForMonth(userId, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            (result.Result as OkObjectResult)!.Value.Should().Be(expected);
        }

        

        [Fact]
        public async Task UpdateTransaction_ReturnsNoContent()
        {
            var dto = new UpdateTransactionDto(
                Guid.NewGuid(),
                200.00m,
                DateTime.UtcNow,
                Guid.NewGuid(),
                "Note updated",
                "Account updated",
                "UpdatedCategory"
            );

            _mockSender.Setup(s => s.Send(It.IsAny<UpdateTransactionCommand>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(Unit.Value));

            var result = await _controller.UpdateTransaction(dto, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteTransaction_ReturnsNoContent()
        {
            var dto = new DeleteTransactionDto(Guid.NewGuid(), Guid.NewGuid());

            _mockSender.Setup(s => s.Send(It.IsAny<DeleteTransactionCommand>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(Unit.Value));

            var result = await _controller.DeleteTransaction(dto, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task GetCashFlow_ReturnsOk_WithCashFlowData()
        {
            var userId = Guid.NewGuid();
            var expected = new List<DetailedDailyCashFlowDto>
            {
                new DetailedDailyCashFlowDto(DateTime.UtcNow, 123.45m, 0m, 0m, 0m)
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetDetailedDailyCashFlowQuery>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expected);

            var result = await _controller.GetCashFlow(userId);

            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetSpendingPerCategory_ReturnsOk_WithCategoryData()
        {
            var userId = Guid.NewGuid();
            var expected = new List<CategoryTotalDto>
            {
                new CategoryTotalDto
                {
                    Category = "Food",
                    TotalAmount = 120.50m
                }
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetSpendingPerCategoryQuery>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expected);

            var result = await _controller.GetSpendingPerCategory(userId);

            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().BeEquivalentTo(expected);
        }
    }
}