namespace BudgetAppBackend.API.Models
{
    public record SyncTransactionsRequest(Guid userId,string AccessToken, string? Cursor, int? Count);
}
