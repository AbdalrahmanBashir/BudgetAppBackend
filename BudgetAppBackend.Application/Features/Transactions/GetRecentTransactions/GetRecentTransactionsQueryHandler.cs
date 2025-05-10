using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetRecentTransactions
{
    public class GetRecentTransactionsQueryHandler : IRequestHandler<GetRecentTransactionsQuery, List<TransactionDto>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _laidTransactionRepository;
        public GetRecentTransactionsQueryHandler(ITransactionRepository transactionRepository, IPlaidTransactionRepository laidTransactionRepository)
        {
            _transactionRepository = transactionRepository;
            _laidTransactionRepository = laidTransactionRepository;
        }
        public async Task<List<TransactionDto>> Handle(GetRecentTransactionsQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.UserId);
            var manualTransactions = await _transactionRepository.GetRecentTransactionsByUserAsync(userId, cancellationToken);
            var plaidTransactions = await _laidTransactionRepository.GetRecentTransactionsByUserAsync(userId, cancellationToken);
            var allTransactions = manualTransactions.Concat(plaidTransactions)
                                   .OrderByDescending(t => t.TransactionDate)
                                   .ToList();

            return allTransactions;
        }
    }
}
