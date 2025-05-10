namespace BudgetAppBackend.Application.DTOs.BudgetDTOs
{
    public class BudgetDto
    {
        public Guid BudgetId { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
