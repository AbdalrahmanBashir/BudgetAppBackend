using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.DeleteTransaction
{
    public class DeleteTransactionCommand : IRequest<Unit>
    {
        public DeleteTransactionDto DeleteTransactionDto { get; set; }
    }
}
