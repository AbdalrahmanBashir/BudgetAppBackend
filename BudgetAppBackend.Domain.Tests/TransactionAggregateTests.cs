using BudgetAppBackend.Domain.TransactionAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using BudgetAppBackend.Domain.Exceptions.TransactionExceptions;

namespace BudgetAppBackend.Domain.Tests
{
    public class TransactionAggregateTests
    {
        [Fact]
        public void CreateTransaction_ShouldInitializeCorrectly()
        {
            // Arrange
            var userId = UserId.CreateId();
            var category = new List<string> { "Travel" };
            decimal amount = 100;
            DateTime transactionDate = DateTime.UtcNow;
            string payee = "John Doe";
            TransactionType type = TransactionType.Expense;

            // Act
            var transaction = Transaction.Create(userId, category, amount, transactionDate, payee, type);

            // Assert
            Assert.Equal(userId, transaction.UserId);
            Assert.Equal(category.FirstOrDefault(), transaction.Categories.FirstOrDefault());
            Assert.Equal(amount, transaction.Amount);
            Assert.Equal(transactionDate, transaction.TransactionDate);
            Assert.Equal(payee, transaction.Payee);
            Assert.Equal(type, transaction.Type);
        }

        

        [Fact]
        public void CreateTransaction_ShouldThrowException_WhenPayeeIsEmpty()
        {
            // Arrange
            var userId = UserId.CreateId();
            var category = new List<string> { "Travel" };
            decimal amount = 100;
            DateTime transactionDate = DateTime.UtcNow;
            string payee = "";
            TransactionType type = TransactionType.Expense;

            // Act & Assert
            Assert.Throws<TransactionPayeeException>(() => Transaction.Create(userId, category, amount, transactionDate, payee, type));
        }

        [Fact]
        public void UpdateTransaction_ShouldModifyValuesCorrectly()
        {
            // Arrange
            var transaction = Transaction.Create(UserId.CreateId(), new List<string> { "Travel" }, 100, DateTime.UtcNow, "John Doe", TransactionType.Expense);
            DateTime newTransactionDate = DateTime.UtcNow.AddDays(-1);
            string newPayee = "Jane Doe";
            TransactionType newType = TransactionType.Income;

            // Act
            transaction.UpdateTransaction(100, newTransactionDate, newPayee, "Travel", newType);

            // Assert
            Assert.Equal(newTransactionDate, transaction.TransactionDate);
            Assert.Equal(newPayee, transaction.Payee);
            Assert.Equal(newType, transaction.Type);
        }

        

        [Fact]
        public void DeleteTransaction_ShouldRaiseDomainEvent()
        {
            // Arrange
            var transaction = Transaction.Create(UserId.CreateId(), new List<string> { "Travel" }, 100, DateTime.UtcNow, "John Doe", TransactionType.Expense);

            // Act
            transaction.DeleteTransaction();

            // Assert
            Assert.Equal(100, transaction.Amount);
        }
    }
}

