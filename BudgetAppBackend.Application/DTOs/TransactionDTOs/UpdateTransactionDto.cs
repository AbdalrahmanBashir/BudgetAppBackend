namespace BudgetAppBackend.Application.DTOs.TransactionDTOs
{
    public record UpdateTransactionDto(
        Guid TransactionId,
        decimal Amount,
        DateTime TransactionDate,
        Guid UserId,
        string payee,
        string transactionType,
        string category
    );
}