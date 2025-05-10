using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Application.DTOs.AuthenticationDTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; }
        public string? Token { get;  set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public string? Message { get; set; }
    }
    
}
