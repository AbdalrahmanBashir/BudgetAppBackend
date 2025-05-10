using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<AuthResult>
    {
        public VerifyEmailDto? VerifyEmailDto { get; set; }
    }
}
