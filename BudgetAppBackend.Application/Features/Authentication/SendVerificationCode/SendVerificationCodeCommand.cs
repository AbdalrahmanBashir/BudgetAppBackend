using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.SendVerificationCode
{
    public class SendVerificationCodeCommand : IRequest<bool>
    {
        public SendVerificationCodeDto SendVerificationCodeDto { get; set; }
    }
}
