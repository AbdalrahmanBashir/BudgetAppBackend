namespace BudgetAppBackend.Application.DTOs.TransactionDTOs
{
    public class TransactionFilterDto
    {
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Payee { get; set; }
        public string? Category { get; set; }
    }
}