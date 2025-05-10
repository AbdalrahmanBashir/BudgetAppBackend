using BudgetAppBackend.Application.DTOs.Reports;

namespace BudgetAppBackend.Application.Contracts
{
    public interface  IPdfReportService
    {
        byte[] GenerateFinancialReport(FinancialReportDto reportData);
    }
}
