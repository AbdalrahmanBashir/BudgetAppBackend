using System.ComponentModel.DataAnnotations;

namespace BudgetAppBackend.Application.DTOs.AuthenticationDTOs
{
    public class VerifyEmailDto
    {
        public required string Email { get; init; }
        public required string Code { get; init; }
    }
}
