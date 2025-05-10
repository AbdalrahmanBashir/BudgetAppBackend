namespace BudgetAppBackend.Application.Models.PlaidModels
{
    public record TransactionsSyncResponse(
        List<PlaidTransactionDto> Added,
        List<PlaidTransactionDto> Modified,
        List<RemovedTransaction> Removed,
        string NextCursor,
        bool HasMore,
        string RequestId,
        string? ItemId = null);

    public record RemovedTransaction(
        string TransactionId,
        string AccountId);

    public record PersonalFinanceCategory(
        string Primary,
        string Detailed,
        string? ConfidenceLevel);

    public record Counterparty(
        string Name,
        string Type,
        string? EntityId,
        string? Website,
        string? LogoUrl,
        string? ConfidenceLevel);

    public record PlaidTransactionDto(
       Guid userId,
       string PlaidTransactionId,
       string AccountId,
       decimal Amount,
       string Name,
       DateTime Date,
       List<string> Categories,
       string? CategoryId,
       string? MerchantName);

}
