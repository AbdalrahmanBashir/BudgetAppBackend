namespace BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
    
public record SpendingForecast(
    double TotalForecastedSpending,
    double AverageMonthlySpending,
    string TrendDirection,
    double PercentageChange,
    string ActionableInsights,
    string Disclaimer
);

