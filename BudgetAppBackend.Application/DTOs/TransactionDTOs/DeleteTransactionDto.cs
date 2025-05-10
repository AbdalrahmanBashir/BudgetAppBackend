namespace BudgetAppBackend.Application.DTOs.TransactionDTOs
{
    public record DeleteTransactionDto(
        Guid TransactionId,
        Guid UserId
    );
}
