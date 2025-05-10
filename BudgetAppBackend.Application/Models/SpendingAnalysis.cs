namespace BudgetAppBackend.Application.Models
{
    public record SpendingAnalysis(
    string Overview,
    string AnomaliesOrRedFlags,
    string Recommendations,
    string Disclaimer);

}
