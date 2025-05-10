using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.GetCategoryTotalsForLastFourMonths
{
    public class GetCategoryTotalsForLastFourMonthsQueryHandler : IRequestHandler<GetCategoryTotalsForLastFourMonthsQuery, List<MonthlyCategoryTotalDto>> // Change return type to List<MonthlyCategoryTotalDto>
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _laidTransactionRepository;

        public GetCategoryTotalsForLastFourMonthsQueryHandler(IBudgetRepository budgetRepository, ITransactionRepository transactionRepository, IPlaidTransactionRepository laidTransactionRepository)
        {
            _budgetRepository = budgetRepository;
            _transactionRepository = transactionRepository;
            _laidTransactionRepository = laidTransactionRepository;
        }

        public async Task<List<MonthlyCategoryTotalDto>> Handle(GetCategoryTotalsForLastFourMonthsQuery request, CancellationToken cancellationToken) // Update return type
        {
            var userId = UserId.Create(request.GetTotalBudgetForLastFourMonthsDto.UserId);
            var budget = await _budgetRepository.GetBudgetAsync(userId, request.GetTotalBudgetForLastFourMonthsDto.Category, cancellationToken); // Add await for async call
            if (budget == null)
            {
                throw new Exception("Budget not found");
            }
            var manu = await _transactionRepository.GetCategoryTotalsForLastFourMonthsAsync(request.GetTotalBudgetForLastFourMonthsDto.Category, userId, cancellationToken);
            var plaid = await _laidTransactionRepository.GetCategoryTotalsForLastFourMonthsAsync(request.GetTotalBudgetForLastFourMonthsDto.Category, userId, cancellationToken);



            var combinedTotals = manu
            .Concat(plaid)
            .GroupBy(dto => dto.Month)
            .Select(group => new MonthlyCategoryTotalDto
            {
                Month = group.Key,
                Total = group.Sum(dto => dto.Total)
            })
            .OrderBy(dto => dto.Month)
            .ToList();

            return combinedTotals; // Return the list
        }
    }
}
