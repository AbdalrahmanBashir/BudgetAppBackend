namespace BudgetAppBackend.Application.Models
{
    public class UserFinancialContext
    {
        public int TransactionCount { get; set; }
        public decimal TotalSpending { get; set; }
        public decimal RecentSpending { get; set; }
    }
}
