using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.PlaidTransactionAggregate;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Xunit;

namespace BudgetAppBackend.Domain.Tests
{
    public class PlaidTransactionTests
    {
        private readonly UserId _userId = UserId.Create(Guid.NewGuid());
        private const string PlaidTransactionId = "TX123";
        private const string AccountId = "ACC456";
        private const string Name = "Test Transaction";
        private static readonly DateTime Date = DateTime.UtcNow.Date;
        private readonly List<string> _categories = new() { "Food", "Groceries" };
        private const string CategoryId = "CAT1";
        private const string MerchantName = "Test Merchant";

        [Fact]
        public void Create_ValidParameters_PropertiesCorrectlySet()
        {
            // Act
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                100m,
                Name,
                Date,
                _categories,
                CategoryId,
                MerchantName);

            // Assert
            Assert.Equal(_userId, transaction.UserId);
            Assert.Equal(PlaidTransactionId, transaction.PlaidTransactionId);
            Assert.Equal(AccountId, transaction.AccountId);
            Assert.Equal(-100m, transaction.Amount);
            Assert.Equal(Name, transaction.Name);
            Assert.Equal(Date, transaction.Date);
            Assert.Equal(_categories, transaction.Categories);
            Assert.Equal(CategoryId, transaction.CategoryId);
            Assert.Equal(MerchantName, transaction.MerchantName);
            Assert.False(transaction.IsRemoved);
            Assert.InRange(transaction.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            Assert.InRange(transaction.LastModifiedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Theory]
        [InlineData(100, -100)]
        [InlineData(-50, 50)]
        [InlineData(0, 0)]
        public void Amount_Normalization_CorrectlyApplied(decimal inputAmount, decimal expectedAmount)
        {
            // Act
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                inputAmount,
                Name,
                Date,
                _categories,
                CategoryId,
                MerchantName);

            // Assert
            Assert.Equal(expectedAmount, transaction.Amount);
        }

        [Fact]
        public void Create_RaisesTransactionCreatedEvent()
        {
            // Act
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                100m,
                Name,
                Date,
                _categories,
                CategoryId,
                MerchantName);

            // Assert
            var domainEvent = Assert.Single(transaction.DomainEvents) as PlaidTransactionCreatedEvent;
            Assert.NotNull(domainEvent);
            Assert.Equal(transaction.Id.Id, domainEvent.TransactionId);
            Assert.Equal(_userId.Id, domainEvent.UserId);
            Assert.Equal(100m, domainEvent.Amount);
            Assert.Equal(-100m, domainEvent.normalizedAmount);
            Assert.Equal("Food", domainEvent.Category);
        }

        

        [Fact]
        public void MarkAsRemoved_SetsFlagAndRaisesEvent()
        {
            // Arrange
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                100m,
                Name,
                Date,
                _categories,
                CategoryId,
                MerchantName);

            transaction.ClearDomainEvents();

            // Act
            transaction.MarkAsRemoved();

            // Assert
            Assert.True(transaction.IsRemoved);
            var domainEvent = Assert.Single(transaction.DomainEvents) as PlaidTransactionRemovedDomainEvent;
            Assert.NotNull(domainEvent);
            Assert.Equal(100m, domainEvent.Amount);
            Assert.Equal("Food", domainEvent.Category);
            Assert.Equal(Date, domainEvent.Date);
        }

        [Fact]
        public void MarkAsRemoved_WhenAlreadyRemoved_DoesNothing()
        {
            // Arrange
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                100m,
                Name,
                Date,
                _categories,
                CategoryId,
                MerchantName);

            transaction.MarkAsRemoved();
            transaction.ClearDomainEvents();

            // Act
            transaction.MarkAsRemoved();

            // Assert
            Assert.Empty(transaction.DomainEvents);
        }

        [Fact]
        public void Update_ChangingCategories_RaisesModifiedEvent()
        {
            // Arrange
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                100m,
                Name,
                Date,
                _categories,
                CategoryId,
                MerchantName);

            transaction.ClearDomainEvents();
            var newCategories = new List<string> { "Utilities", "Services" };

            // Act
            transaction.Update(
                transaction.Amount,
                transaction.Name,
                transaction.Date,
                newCategories,
                transaction.CategoryId,
                transaction.MerchantName);

            // Assert
            var domainEvent = Assert.Single(transaction.DomainEvents) as PlaidTransactionModifiedDomainEvent;
            Assert.Equal("Food", domainEvent.oldCategory);
            Assert.Equal("Utilities", domainEvent.Category);
        }

        [Fact]
        public void Update_ChangingToEmptyCategories_HandlesCorrectly()
        {
            // Arrange
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                100m,
                Name,
                Date,
                _categories,
                CategoryId,
                MerchantName);

            // Act
            transaction.Update(
                transaction.Amount,
                transaction.Name,
                transaction.Date,
                new List<string>(),
                transaction.CategoryId,
                transaction.MerchantName);

            // Assert
            Assert.Empty(transaction.Categories);
        }

        [Fact]
        public void Create_WithNullCategories_InitializesEmptyList()
        {
            // Arrange & Act
            var transaction = PlaidTransaction.Create(
                _userId,
                PlaidTransactionId,
                AccountId,
                100m,
                Name,
                Date,
                new List<string>(), // Use empty list instead of null
                CategoryId,
                MerchantName);

            // Assert
            Assert.NotNull(transaction.Categories);
            Assert.Empty(transaction.Categories);
        }
    }
}