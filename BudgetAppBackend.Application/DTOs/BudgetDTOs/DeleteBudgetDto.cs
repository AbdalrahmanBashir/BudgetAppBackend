namespace BudgetAppBackend.Application.DTOs.BudgetDTOs
{
    public class DeleteBudgetDto
    {
        public Guid BudgetId { get; set; }
        public Guid UserId { get; set; }
    }
}
