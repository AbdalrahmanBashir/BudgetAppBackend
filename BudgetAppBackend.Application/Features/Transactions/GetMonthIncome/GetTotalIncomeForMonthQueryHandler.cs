using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetMonthIncome
{
    public class GetTotalIncomeForMonthQueryHandler : IRequestHandler<GetTotalIncomeForMonthQuery, decimal>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _laidTransactionRepository;

        public GetTotalIncomeForMonthQueryHandler(ITransactionRepository transactionRepository, IPlaidTransactionRepository laidTransactionRepository)
        {
            _transactionRepository = transactionRepository;
            _laidTransactionRepository = laidTransactionRepository;
        }
        public async Task<decimal> Handle(GetTotalIncomeForMonthQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.UserId);
            var currentDate = DateTime.UtcNow;
            var plaidIncome = await _laidTransactionRepository.GetTotalIncomeForMonthAsync(userId, currentDate, cancellationToken);
            var manualIncome = await _transactionRepository.GetTotalIncomeForMonthAsync(userId, currentDate, cancellationToken);

            return manualIncome+ plaidIncome;
        }
    }
}
