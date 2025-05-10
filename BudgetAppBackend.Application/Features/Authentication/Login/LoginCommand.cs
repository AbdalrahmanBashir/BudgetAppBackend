using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.Login
{
    public class LoginCommand : IRequest<AuthResult>
    {
        public LogUserDto? LogUser { get; set; }
    }
}
