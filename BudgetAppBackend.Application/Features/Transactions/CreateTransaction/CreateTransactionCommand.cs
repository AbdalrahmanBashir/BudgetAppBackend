using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.CreateTransaction
{
    public class CreateTransactionCommand : IRequest<Unit>
    {
        public CreateTransactionDto createTransactionDto { get; set; }
    }
}
