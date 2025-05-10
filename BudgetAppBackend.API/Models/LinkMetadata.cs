namespace BudgetAppBackend.API.Models
{
    public class LinkMetadata
    {
        public string InstitutionId { get; set; } = null!;
        public string InstitutionName { get; set; } = null!;
        public List<AccountMetadata> Accounts { get; set; } = new();
        public string? LinkSessionId { get; set; }
    }
}
