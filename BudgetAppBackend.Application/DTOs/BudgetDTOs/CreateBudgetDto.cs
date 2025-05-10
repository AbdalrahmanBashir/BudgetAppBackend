namespace BudgetAppBackend.Application.DTOs.BudgetDTOs
{
    public class CreateBudgetDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public decimal TotalAmount { get; set; }
        public string Category { get; set; }
    }
}
