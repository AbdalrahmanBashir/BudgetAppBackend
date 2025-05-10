namespace BudgetAppBackend.Application.DTOs.BudgetDTOs
{
    public class GetTopFiveTransactionsRequest
    {
        public Guid UserId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
