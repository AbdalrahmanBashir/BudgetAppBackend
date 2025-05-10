namespace BudgetAppBackend.Application.DTOs.BudgetDTOs
{
    public class MonthlyCategoryTotalDto
    {
        public string Month { get; set; } = default!;
        public decimal Total { get; set; }
    }
}
