using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthResult>
    {
        private readonly IAuthRepository _authRepository;

        public ResetPasswordCommandHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<AuthResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            
            var user = await _authRepository.GetUserByEmailAsync(request.resetPassword.Email, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email address.");
            }

            if (request.resetPassword.NewPassword != request.resetPassword.ConfirmNewPassword)
            {
                throw new ArgumentException("Passwords do not match.");
            }

            user.ChangePassword(request.resetPassword.NewPassword);

            await _authRepository.UpdateUserAsync(user, cancellationToken);

            return new AuthResult { Success = true, Message = "Password reset successfully." };
        }
    }
}
