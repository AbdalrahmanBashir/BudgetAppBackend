namespace BudgetAppBackend.Application.DTOs.AiAnalysisDTOS
{
    public class DailySpendingForecastResult
    {
        public List<float> Forecast { get; set; } = new();
        public DateTime StartDate { get; set; }
    }
}
