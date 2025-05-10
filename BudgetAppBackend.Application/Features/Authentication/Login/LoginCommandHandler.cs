using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.UserAggregate.Entities;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IAuthService _authenticationService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPlaidAccountFingerprintRepository _plaidAccountFingerprintRepository;

        public LoginCommandHandler(IAuthRepository authRepository, IAuthService authenticationService,
            IRefreshTokenRepository refreshTokenRepository,
            IPlaidAccountFingerprintRepository plaidAccountFingerprintRepository)
        {
            _authRepository = authRepository;
            _authenticationService = authenticationService;
            _refreshTokenRepository = refreshTokenRepository;
            _plaidAccountFingerprintRepository = plaidAccountFingerprintRepository;
        }

        public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
           
            var user = await _authRepository.GetUserByEmailAsync(request.LogUser!.Email!, cancellationToken);
            if (user == null || !user.VerifyPassword(request.LogUser.Password!))
            {
                throw new UnauthorizedAccessException("Invalid email or password. Please check your credentials and try again.");
            }
            if (!user.IsEmailVerified)
            {
                throw new UnauthorizedAccessException("Email not verified. Please verify your email address to log in.");
            }


            var token = _authenticationService.GenerateToken(user);
            var (rawRefreshToken, hashedRefreshToken) = _authenticationService.GenerateRefreshToken();
            var existingRefreshToken = await _refreshTokenRepository.GetByUserIdAsync(user.Id, cancellationToken);


            if (existingRefreshToken != null && existingRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                existingRefreshToken.Revoke();
                await _refreshTokenRepository.UpdateAsync(existingRefreshToken, cancellationToken);
                existingRefreshToken = null;
            }

            DateTime newRefreshTokenExpiry = (existingRefreshToken != null && existingRefreshToken.ExpiryDate > DateTime.UtcNow)
                    ? existingRefreshToken.ExpiryDate
                    : DateTime.UtcNow.AddMinutes(160);
            var newRefreshToken = new RefreshToken(user.Id, hashedRefreshToken, newRefreshTokenExpiry);

            if (existingRefreshToken != null)
            {
                existingRefreshToken.Revoke();
                await _refreshTokenRepository.UpdateAndSaveNewAsync(existingRefreshToken, newRefreshToken, cancellationToken);
            }
            else
            {
                await _refreshTokenRepository.SaveAsync(newRefreshToken, cancellationToken);
                
            }

            var plaidAccounts = await _plaidAccountFingerprintRepository.GetByUserIdAsync(user.Id);
            var topPlaidAccount = plaidAccounts
                .OrderByDescending(p => p.LastUpdated)
                .FirstOrDefault();


            return new AuthResult
            {
                Success = true,
                UserId = user.Id.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Token = token,
                RefreshToken = rawRefreshToken,
                RefreshTokenExpiry = newRefreshTokenExpiry,
                Message = "You have successfully logged in! Welcome back.",
                PlaidToken = topPlaidAccount?.Fingerprint,
            };
        }
    }
}
