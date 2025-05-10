using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;

namespace BudgetAppBackend.Application.DTOs.Reports
{
    public class FinancialReportDto
    {
        public string UserFullName { get; set; } // Added user name
        public DateTime ReportDate { get; set; } // Added report date
        public string ReportPeriod { get; set; } // Added report period
        public Guid ReportId { get; set; } // Added report ID
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetSavings => TotalIncome - TotalExpenses;
        public List<BudgetDto> Budgets { get; set; }
        public List<TransactionDto> RecentTransactions { get; set; }
    }
}
