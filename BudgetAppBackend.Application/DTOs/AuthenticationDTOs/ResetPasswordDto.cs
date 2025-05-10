using System.ComponentModel.DataAnnotations;

namespace BudgetAppBackend.Application.DTOs.AuthenticationDTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; init; }
        public string NewPassword { get; init; }
        public string ConfirmNewPassword { get; init; }
    }
}
