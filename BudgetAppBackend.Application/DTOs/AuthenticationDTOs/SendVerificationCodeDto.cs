using System.ComponentModel.DataAnnotations;

namespace BudgetAppBackend.Application.DTOs.AuthenticationDTOs
{
    public class SendVerificationCodeDto
    {  
        public string Email { get; init; }
    }
}
