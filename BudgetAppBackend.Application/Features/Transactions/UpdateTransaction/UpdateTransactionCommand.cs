using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.UpdateTransaction
{
    public class UpdateTransactionCommand : IRequest<Unit>
    {
        public UpdateTransactionDto UpdateTransactionDto { get; set; }
    }
}
