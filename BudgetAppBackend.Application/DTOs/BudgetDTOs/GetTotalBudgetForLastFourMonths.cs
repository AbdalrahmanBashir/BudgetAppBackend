namespace BudgetAppBackend.Application.DTOs.BudgetDTOs
{
    public class GetTotalBudgetForLastFourMonths
    {
        public Guid UserId { get; set; }

        public string Category { get; set; }
    }
}
