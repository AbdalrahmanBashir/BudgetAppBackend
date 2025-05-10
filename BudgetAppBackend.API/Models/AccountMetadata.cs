namespace BudgetAppBackend.API.Models
{
    public class AccountMetadata
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Mask { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Subtype { get; set; }
    }
}
