using System.Text;
using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Konscious.Security.Cryptography;

namespace BudgetAppBackend.Domain.UserAggregate
{
    public sealed class User : AggregateRoot<UserId>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public byte[] PasswordSalt { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public string? EmailVerificationCode { get; private set; }
        public DateTime? EmailVerificationCodeExpiry { get; private set; }

        private User() : base(default!)
        {
            // It's mainly used for ORM frameworks that require a parameterless constructor.
        }

        private User(UserId id, string firstName, string lastName, string email, byte[] passwordHash, byte[] passwordSalt) : base(id)
        {
            FirstName = ValidateString(firstName, nameof(FirstName));
            LastName = ValidateString(lastName, nameof(LastName));
            Email = ValidateEmail(email);
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            PasswordSalt = passwordSalt ?? throw new ArgumentNullException(nameof(passwordSalt));
            IsEmailVerified = false;
        }

        public static User CreateNewUser(string firstName, string lastName, string email, string password)
        {
            var passwordData = GeneratePasswordHash(password);

            return new User(
                UserId.CreateId(),
                firstName,
                lastName,
                email,
                passwordData.PasswordHash,
                passwordData.PasswordSalt
            );
        }


        public void ChangePassword(string newPassword)
        {
            var (newPasswordHash, newPasswordSalt) = GeneratePasswordHash(newPassword);
            PasswordHash = newPasswordHash;
            PasswordSalt = newPasswordSalt;

            RaiseDomainEvent(new PasswordChangedEvent(Id.Id, LastName, Email));
        }


        private static string ValidateString(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{propertyName} cannot be null or empty.", propertyName);
            return value;
        }

        private static string ValidateEmail(string email)
        {
            if (!email.Contains("@"))
                throw new ArgumentException("Invalid email format.", nameof(email));
            return email;
        }

        public static string GenerateVerificationToken()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public void SetEmailVerificationCode(string code, DateTime expiry, string firstName,string lastName, string email)
        {
            EmailVerificationCode = code;
            EmailVerificationCodeExpiry = expiry;

            RaiseDomainEvent(new EmailVerificationCodeGeneratedEvent(code, expiry, firstName, lastName, email));
        }

        public bool VerifyEmail(string code)
        {
            if (code == EmailVerificationCode && EmailVerificationCodeExpiry > DateTime.UtcNow)
            {
                IsEmailVerified = true;
                EmailVerificationCode = null;
                EmailVerificationCodeExpiry = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static (byte[] PasswordHash, byte[] PasswordSalt) GeneratePasswordHash(string password)
        {
            byte[] passwordSalt = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(passwordSalt);
            }

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = passwordSalt;
            argon2.DegreeOfParallelism = 8;
            argon2.MemorySize = 65536;
            argon2.Iterations = 4;

            byte[] passwordHash = argon2.GetBytes(64);

            return (passwordHash, passwordSalt);
        }

        public bool VerifyPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null.");
            }
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = PasswordSalt,
                DegreeOfParallelism = 8,
                MemorySize = 65536,
                Iterations = 4
            };

            var computedHash = argon2.GetBytes(64);
            return AreHashesEqual(computedHash, PasswordHash);
        }

        private static bool AreHashesEqual(byte[] computedHash, byte[] storedHash)
        {
            if (computedHash.Length != storedHash.Length) return false;

            bool areEqual = true;
            for (int i = 0; i < computedHash.Length; i++)
            {
                areEqual &= (computedHash[i] == storedHash[i]);
            }

            return areEqual;
        }

    }
}
