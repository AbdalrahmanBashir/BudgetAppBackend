namespace BudgetAppBackend.Application.DTOs.TransactionDTOs;
public record DetailedDailyCashFlowDto(
    DateTime Date,
    decimal Income,
    decimal Expense,
    decimal NetCashFlow,
    decimal CumulativeCashFlow);

