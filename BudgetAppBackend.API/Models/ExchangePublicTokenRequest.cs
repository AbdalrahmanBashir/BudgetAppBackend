namespace BudgetAppBackend.API.Models
{
    public class ExchangePublicTokenRequest
    {
        public string PublicToken { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public LinkMetadata? Metadata { get; set; }
    }
}
