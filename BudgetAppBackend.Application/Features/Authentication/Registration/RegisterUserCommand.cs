using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.Registration
{
    public class RegisterUserCommand : IRequest<AuthResult>
    {
        public AddUserDto? AddUser { get; set; }
    }
}
