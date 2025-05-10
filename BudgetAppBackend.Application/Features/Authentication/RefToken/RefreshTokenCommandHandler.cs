using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.UserAggregate.Entities;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace BudgetAppBackend.Application.Features.Authentication.RefToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuthService _authService;
        private readonly IAuthRepository _authRepository;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IAuthService authService,
            IAuthRepository authRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _authService = authService;
            _authRepository = authRepository;
        }

        public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            var existingRefreshToken = await _refreshTokenRepository.GetByUserIdAsync(user.Id, cancellationToken);

            if (existingRefreshToken == null)
            {
                throw new UnauthorizedAccessException("Refresh token not found.");
            }

            if (existingRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                existingRefreshToken.Revoke();
                await _refreshTokenRepository.UpdateAsync(existingRefreshToken, cancellationToken);
                throw new SecurityTokenExpiredException("Refresh token expired. Please log in again.");
            }

            if (!_authService.ValidateRefreshToken(existingRefreshToken.TokenHash, request.Token))
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

           
            var newToken = _authService.GenerateToken(user);
            var (rawRefreshToken, hashedRefreshToken) = _authService.GenerateRefreshToken();
            DateTime newRefreshTokenExpiry = existingRefreshToken.ExpiryDate > DateTime.UtcNow
                ? existingRefreshToken.ExpiryDate : DateTime.UtcNow.AddMinutes(2);

            existingRefreshToken.Revoke();
            var newRefreshToken = new RefreshToken(user.Id, hashedRefreshToken, newRefreshTokenExpiry);
            await _refreshTokenRepository.UpdateAndSaveNewAsync(existingRefreshToken, newRefreshToken, cancellationToken);

            return new AuthResult
            {
                Success = true,
                Token = newToken,
                RefreshToken = rawRefreshToken,
                RefreshTokenExpiry = newRefreshTokenExpiry,
                Message = "You logged in successfully"
            };
        }
    }
}
