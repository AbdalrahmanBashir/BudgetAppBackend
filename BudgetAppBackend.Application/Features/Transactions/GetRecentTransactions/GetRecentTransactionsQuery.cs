using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetRecentTransactions
{
    public class GetRecentTransactionsQuery : IRequest<List<TransactionDto>>
    {
        public Guid UserId { get; set; }
    }
}
