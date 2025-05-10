using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Application.DTOs.PagingsDTOs;
using MediatR;


namespace BudgetAppBackend.Application.Features.Transactions.GetUserTransactionsWithPagination
{
    public class GetUserTransactionsWithPaginationQuery : IRequest<PagedResponse <TransactionDto>>
    {
        public Guid? UserId { get; set; }
        public PagingDTO? Paging { get; set; }
        public TransactionFilterDto? Filter { get; set; }
    }
}
