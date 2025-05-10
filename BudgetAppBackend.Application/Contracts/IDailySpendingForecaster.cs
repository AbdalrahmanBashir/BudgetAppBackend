using BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;

namespace BudgetAppBackend.Application.Contracts
{
    public interface IDailySpendingForecaster
    {
        Task<DailySpendingForecastResult> ForecastAsync(float[] dailyAmounts, DateTime lastKnownDate, int daysToPredict);
    }
}
