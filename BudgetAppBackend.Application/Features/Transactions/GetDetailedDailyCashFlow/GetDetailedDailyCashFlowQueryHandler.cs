using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetDetailedDailyCashFlow
{
    public class GetDetailedDailyCashFlowQueryHandler : IRequestHandler<GetDetailedDailyCashFlowQuery, IEnumerable<DetailedDailyCashFlowDto>>
    {
        private readonly ITransactionRepository _transactionReadRepository;
        private readonly IPlaidTransactionRepository _plaidTransactionRepository;
        public GetDetailedDailyCashFlowQueryHandler(
            ITransactionRepository transactionReadRepository,
            IPlaidTransactionRepository plaidTransactionRepository)
        {
            _transactionReadRepository = transactionReadRepository;
            _plaidTransactionRepository = plaidTransactionRepository;
        }
        public async Task<IEnumerable<DetailedDailyCashFlowDto>> Handle(GetDetailedDailyCashFlowQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.UserId);
            var monthStartDate = DateTime.UtcNow.AddDays(-30);  // from today go back 30 days.
            var manualTransactions = await _transactionReadRepository.GetDetailedDailyCashFlowAsync(userId, monthStartDate, cancellationToken);
            var plaidTransactions = await _plaidTransactionRepository.GetDetailedDailyCashFlowAsync(userId, monthStartDate, cancellationToken);

            // Combine and process transactions here
            var combinedTransactions = manualTransactions.Concat(plaidTransactions)
                .OrderByDescending(t => t.Date)
                                   .ToList();

            return combinedTransactions;
        }
    }
    
    
}
