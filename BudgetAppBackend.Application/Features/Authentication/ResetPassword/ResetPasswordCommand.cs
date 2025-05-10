using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.ResetPassword
{
    public class ResetPasswordCommand : IRequest<AuthResult>
    {
        public ResetPasswordDto resetPassword { get; set; }
    }
}
