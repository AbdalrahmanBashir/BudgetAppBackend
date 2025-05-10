using AutoMapper;
using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.GetBudgetsByUser
{
    public class GetBudgetsByUserQueryHandler : IRequestHandler<GetBudgetsByUserQuery, List<BudgetDto>>
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IMapper _mapper;

        public GetBudgetsByUserQueryHandler(IBudgetRepository budgetRepository, IMapper mapper)
        {
            _budgetRepository = budgetRepository;
            _mapper = mapper;
        }
        public async Task<List<BudgetDto>> Handle(GetBudgetsByUserQuery request, CancellationToken cancellationToken)
        {
             var userId = UserId.Create(request.UserId);
            var budgets = await _budgetRepository.GetActiveBudgetsAsync(userId, cancellationToken);

               var mappedBudgets = _mapper.Map<List<BudgetDto>>(budgets);

               return mappedBudgets;
        }
    }
}
