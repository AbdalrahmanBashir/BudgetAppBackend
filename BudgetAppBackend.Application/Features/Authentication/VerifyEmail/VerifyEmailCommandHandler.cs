using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, AuthResult>
    {
        private readonly IAuthRepository _authRepository;

        public VerifyEmailCommandHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<AuthResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
           

            var user = await _authRepository.GetUserByEmailAsync(request.VerifyEmailDto.Email, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!user.VerifyEmail(request.VerifyEmailDto.Code))
            {
                throw new UnauthorizedAccessException("Verification code does not match.");
            }

            await _authRepository.UpdateUserAsync(user, cancellationToken);

            return new AuthResult { Success = true, Message = "Your email has been successfully verified! Welcome to BudgetApp. " +
              "You can now log in and start managing your finances with ease. "
            };
        }
    }
}
