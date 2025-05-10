namespace BudgetAppBackend.Application.DTOs.TransactionDTOs
{
    public record CreateTransactionDto(
        decimal Amount,
        DateTime TransactionDate,
        Guid UserId,
        List<string> Categories,
        string payee,
        string transactionType
    );
}
