using BudgetAppBackend.API.Controllers;
using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.Features.Budgets.CreateBudget;
using BudgetAppBackend.Application.Features.Budgets.DeleteBudget;
using BudgetAppBackend.Application.Features.Budgets.GetBudgetsByUser;
using BudgetAppBackend.Application.Features.Budgets.UpdateBudget;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BudgetAppBackend.API.Tests
{
    public class BudgetControllerTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly BudgetController _controller;

        public BudgetControllerTests()
        {
            _mockSender = new Mock<ISender>();
            _controller = new BudgetController(_mockSender.Object);
        }

        [Fact]
        public async Task CreateBudget_ShouldReturnOkResult_WhenValidDtoProvided()
        {
            // Arrange
            var dto = new CreateBudgetDto
            {
                UserId = Guid.NewGuid(),
                Title = "Test Budget",
                TotalAmount = 1000,
                Category = "Food"
            };

            _mockSender
                .Setup(s => s.Send(It.Is<CreateBudgetCommand>(cmd => cmd.CreateBudgetDto == dto), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.CreateBudget(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(Unit.Value, okResult.Value);

            _mockSender.Verify(s => s.Send(It.Is<CreateBudgetCommand>(cmd => cmd.CreateBudgetDto == dto), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBudget_ShouldReturnOkResult_WhenValidDtoProvided()
        {
            // Arrange
            var dto = new UpdateBudgetDto
            {
                BudgetId = Guid.NewGuid(),
                Title = "Updated Budget",
                TotalAmount = 1500
            };

            _mockSender
                .Setup(s => s.Send(It.Is<UpdateBudgetCommand>(cmd => cmd.UpdateBudgetDto == dto), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.UpdateBudget(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(Unit.Value, okResult.Value);

            _mockSender.Verify(s => s.Send(It.Is<UpdateBudgetCommand>(cmd => cmd.UpdateBudgetDto == dto), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteBudget_ShouldReturnOkResult_WhenValidDtoProvided()
        {
            // Arrange
            var dto = new DeleteBudgetDto
            {
                BudgetId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _mockSender
                .Setup(s => s.Send(It.Is<DeleteBudgetCommand>(cmd => cmd.DeleteBudgetDto == dto), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.DeleteBudget(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(Unit.Value, okResult.Value);

            _mockSender.Verify(s => s.Send(It.Is<DeleteBudgetCommand>(cmd => cmd.DeleteBudgetDto == dto), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetBudgetsByUser_ShouldReturnOkResultWithList_WhenValidUserIdProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedResponse = new List<BudgetDto>
    {
        new BudgetDto { BudgetId = Guid.NewGuid(), Title = "Budget 1", TotalAmount = 500 },
        new BudgetDto { BudgetId = Guid.NewGuid(), Title = "Budget 2", TotalAmount = 1200 }
    };

            _mockSender
                .Setup(s => s.Send(It.Is<GetBudgetsByUserQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetBudgetsByUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsAssignableFrom<List<BudgetDto>>(okResult.Value);
            Assert.Equal(expectedResponse.Count, actual.Count);
            Assert.Equal(expectedResponse[0].Title, actual[0].Title);
            Assert.Equal(expectedResponse[1].TotalAmount, actual[1].TotalAmount);

            _mockSender.Verify(s => s.Send(It.Is<GetBudgetsByUserQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
