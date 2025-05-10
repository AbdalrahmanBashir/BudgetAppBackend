namespace BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
public record QuarterlyTransactionAnalysis(
    string Overview,
    string SpendingTrends,
    string CategoryAnalysis,
    string AnomaliesOrRedFlags,
    string TimeBasedInsights,
    string Recommendations,
    string RiskAssessment,
    string Opportunities,
    string FutureProjections,
    string ComparativeAnalysis,
    string Disclaimer
);