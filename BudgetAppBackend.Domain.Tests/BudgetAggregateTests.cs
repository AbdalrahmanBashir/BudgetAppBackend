using BudgetAppBackend.Domain.BudgetAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using BudgetAppBackend.Domain.Exceptions.BudgetExceptions;

namespace BudgetAppBackend.Domain.Tests
{
    public class BudgetAggregateTests
    {
        [Fact]
        public void CreateBudget_ShouldInitializeCorrectly()
        {
            // Arrange
            var userId = UserId.CreateId();
            var categoryId = "test";
            string title = "Monthly Budget";
            decimal totalAmount = 1000;
            DateTime createdDate = DateTime.UtcNow;

            // Act
            var budget = Budget.Create(userId, title, totalAmount, categoryId, createdDate);

            // Assert
            Assert.Equal(userId, budget.UserId);
            Assert.Equal(categoryId, budget.Category);
            Assert.Equal(title, budget.Title);
            Assert.Equal(totalAmount, budget.TotalAmount);
            Assert.True(budget.IsActive);
        }

        [Fact]
        public void ApplyTransaction_ShouldIncreaseSpentAmount()
        {
            // Arrange
            var budget = Budget.Create(UserId.CreateId(), "Test Budget", 500, "Travel", DateTime.UtcNow);
            decimal transactionAmount = -200;

            // Act
            budget.ApplyTransaction(transactionAmount);

            // Assert
            Assert.Equal(300, budget.GetRemainingBalance());
        }

        [Fact]
        public void ApplyTransaction_ShouldRaiseEvent_WhenBudgetExceeded()
        {
            // Arrange
            var budget = Budget.Create(UserId.CreateId(), "Over Budget", 100, "Travel", DateTime.UtcNow);
            decimal transactionAmount = -150;

            // Act
            budget.ApplyTransaction(transactionAmount);

            // Assert
            Assert.True(budget.HasExceededBudget());
        }

        [Fact]
        public void RollbackTransaction_ShouldDecreaseSpentAmount()
        {
            // Arrange
            var budget = Budget.Create(UserId.CreateId(), "Rollback Budget", 500, "Travel", DateTime.UtcNow);
            budget.ApplyTransaction(-200);

            // Act
            budget.RollbackTransaction(-100);

            // Assert
            Assert.Equal(400, budget.GetRemainingBalance());
        }

        [Fact]
        public void RollbackTransaction_ShouldThrowException_WhenAmountExceedsSpent()
        {
            // Arrange
            var budget = Budget.Create(UserId.CreateId(), "Rollback Fail Budget", 500, "Travel", DateTime.UtcNow);
            budget.ApplyTransaction(-100);

            // Act & Assert
            Assert.Throws<BudgetRollbackException>(() => budget.RollbackTransaction(-150));
        }

        [Fact]
        public void UpdateTotalAmount_ShouldThrowException_WhenNewAmountIsLessThanSpent()
        {
            // Arrange
            var budget = Budget.Create(UserId.CreateId(), "Update Fail Budget", 500, "Travel", DateTime.UtcNow);
            budget.ApplyTransaction(-400);

            // Act & Assert
            Assert.Throws<BudgetDecreaseAmountException>(() => budget.UpdateTotalAmount(300));
        }
    }
}

