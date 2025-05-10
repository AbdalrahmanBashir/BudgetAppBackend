using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.RefToken
{
    public class RefreshTokenCommand : IRequest<AuthResult>
    {
        public string Email { get; set; }
        public string? Token { get; set; }
    }
}
