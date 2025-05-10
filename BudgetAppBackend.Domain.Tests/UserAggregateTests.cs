using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate;

namespace BudgetAppBackend.Domain.Tests
{
    public class UserAggregateTests
    {
        [Fact]
        public void CreateNewUser_ShouldCreateUser_WithValidInputs()
        {
            // Arrange
            string firstName = "Bashir";
            string lastName = "Bashir";
            string email = "Bashir.Bashir@example.com";
            string password = "Password123!";

            // Act
            var user = User.CreateNewUser(firstName, lastName, email, password);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(firstName, user.FirstName);
            Assert.Equal(lastName, user.LastName);
            Assert.Equal(email, user.Email);
            Assert.False(user.IsEmailVerified);
        }

        [Fact]
        public void ChangePassword_ShouldUpdatePasswordHash()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "OldPassword123!");
            string newPassword = "NewSecurePassword!";
            var oldPasswordHash = user.PasswordHash;

            // Act
            user.ChangePassword(newPassword);

            // Assert
            Assert.NotEqual(oldPasswordHash, user.PasswordHash);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
        {
            // Arrange
            string password = "SecurePassword123!";
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", password);

            // Act
            bool result = user.VerifyPassword(password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");

            // Act
            bool result = user.VerifyPassword("WrongPassword!");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetEmailVerificationCode_ShouldRaiseEvent()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");
            string verificationCode = "123456";
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            // Act
            user.SetEmailVerificationCode(verificationCode, expiry, user.FirstName, user.LastName, user.Email);

            // Assert
            Assert.Equal(verificationCode, user.EmailVerificationCode);
            Assert.Equal(expiry, user.EmailVerificationCodeExpiry);
        }

        [Fact]
        public void VerifyEmail_ShouldReturnTrue_ForValidCodeAndNotExpired()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");
            string verificationCode = "123456";
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);
            user.SetEmailVerificationCode(verificationCode, expiry, user.FirstName, user.LastName, user.Email);

            // Act
            bool result = user.VerifyEmail(verificationCode);

            // Assert
            Assert.True(result);
            Assert.True(user.IsEmailVerified);
            Assert.Null(user.EmailVerificationCode);
            Assert.Null(user.EmailVerificationCodeExpiry);
        }

        [Fact]
        public void VerifyEmail_ShouldReturnFalse_ForInvalidCode()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");
            user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(10), user.FirstName, user.LastName, user.Email);

            // Act
            bool result = user.VerifyEmail("654321");

            // Assert
            Assert.False(result);
            Assert.False(user.IsEmailVerified);
        }

        [Fact]
        public void VerifyEmail_ShouldReturnFalse_ForExpiredCode()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");
            user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(-1), user.FirstName, user.LastName, user.Email);

            // Act
            bool result = user.VerifyEmail("123456");

            // Assert
            Assert.False(result);
            Assert.False(user.IsEmailVerified);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateNewUser_InvalidFirstName_ThrowsArgumentException(string invalidFirstName)
        {
            // Arrange
            string lastName = "Bashir";
            string email = "Bashir.Bashir@example.com";
            string password = "Password123!";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => User.CreateNewUser(invalidFirstName, lastName, email, password));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateNewUser_InvalidLastName_ThrowsArgumentException(string invalidLastName)
        {
            // Arrange
            string firstName = "Bashir";
            string email = "Bashir.Bashir@example.com";
            string password = "Password123!";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => User.CreateNewUser(firstName, invalidLastName, email, password));
        }


        [Fact]
        public void GenerateVerificationToken_ReturnsSixDigitNumber()
        {
            // Act
            string token = User.GenerateVerificationToken();

            // Assert
            Assert.NotNull(token);
            Assert.Equal(6, token.Length);
            Assert.True(int.TryParse(token, out _));
        }

        [Fact]
        public void SetEmailVerificationCode_RaisesEventWithCorrectDetails()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");
            string code = "123456";
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);
            string firstName = user.FirstName;
            string lastName = user.LastName;
            string email = user.Email;

            // Act
            user.SetEmailVerificationCode(code, expiry, firstName, lastName, email);

            // Assert
            var raisedEvent = user.DomainEvents.OfType<EmailVerificationCodeGeneratedEvent>().SingleOrDefault();
            Assert.NotNull(raisedEvent);
            Assert.Equal(code, raisedEvent.code);
            Assert.Equal(expiry, raisedEvent.expiry);
            Assert.Equal(firstName, raisedEvent.firstName);
            Assert.Equal(lastName, raisedEvent.lastName);
            Assert.Equal(email, raisedEvent.email);
        }

        [Fact]
        public void ChangePassword_WithSamePassword_GeneratesNewHashAndSalt()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "Password123!");
            var oldHash = user.PasswordHash;
            var oldSalt = user.PasswordSalt;
            string samePassword = "Password123!";

            // Act
            user.ChangePassword(samePassword);

            // Assert
            Assert.NotEqual(oldHash, user.PasswordHash);
            Assert.NotEqual(oldSalt, user.PasswordSalt);
        }



        [Fact]
        public void VerifyEmail_WhenCodeIsNull_ReturnsFalse()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");
            user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(10), "Bashir", "Bashir", "Bashir.Bashir@example.com");
            user.VerifyEmail("123456");

            // Act
            bool result = user.VerifyEmail("123456");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyEmail_ExpiryExactlyNow_ReturnsFalse()
        {
            // Arrange
            var user = User.CreateNewUser("Bashir", "Bashir", "Bashir.Bashir@example.com", "SecurePassword123!");
            DateTime expiry = DateTime.UtcNow;
            user.SetEmailVerificationCode("123456", expiry, "Bashir", "Bashir", "Bashir.Bashir@example.com");

            // Act
            bool result = user.VerifyEmail("123456");

            // Assert
            Assert.False(result);
        }
    }
}

