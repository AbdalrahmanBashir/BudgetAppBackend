namespace BudgetAppBackend.Application.DTOs.BudgetDTOs
{
    public class UpdateBudgetDto
    {
        public Guid BudgetId { get; set; }
        public string Title { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
