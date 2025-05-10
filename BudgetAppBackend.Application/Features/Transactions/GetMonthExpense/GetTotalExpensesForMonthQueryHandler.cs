using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetMonthExpense
{
    public class GetTotalExpensesForMonthQueryHandler : IRequestHandler<GetTotalExpensesForMonthQuery, decimal>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _laidTransactionRepository;

        public GetTotalExpensesForMonthQueryHandler(ITransactionRepository transactionRepository, IPlaidTransactionRepository laidTransactionRepository)
        {
            _transactionRepository = transactionRepository;
            _laidTransactionRepository = laidTransactionRepository;
        }
        public async Task<decimal> Handle(GetTotalExpensesForMonthQuery request, CancellationToken cancellationToken)
        {
            var userId =  UserId.Create(request.UserId);
            var currentDate = DateTime.UtcNow;
            var manulexp = await _transactionRepository.GetTotalExpensesForMonthAsync(userId, currentDate, cancellationToken);
            var plaidExp = await _laidTransactionRepository.GetTotalExpensesForMonthAsync(userId, currentDate, cancellationToken);

            var expense = manulexp + plaidExp;
            return -expense;
        }
    }
}
