using BudgetAppBackend.Domain.UserAggregate;

namespace BudgetAppBackend.Application.Service
{
    public interface IAuthService
    {
        string GenerateToken(User user);
        (string RawToken, string HashedToken) GenerateRefreshToken();
        bool ValidateRefreshToken(string storedHashedToken, string providedRawToken);
    }
}
