using System.ComponentModel.DataAnnotations;

namespace BudgetAppBackend.Application.DTOs.AuthenticationDTOs
{
    public class LogUserDto
    {
        public string? Email { get; init; }
        public string? Password { get; init; }
    }
}
