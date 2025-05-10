using BudgetAppBackend.Domain.UserAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Application.Contracts
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);
        Task SaveAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task DeleteAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task UpdateAndSaveNewAsync(RefreshToken oldToken, RefreshToken newToken, CancellationToken cancellationToken);
    }
}
