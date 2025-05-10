using AutoMapper;
using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.BudgetAggregate;
using BudgetAppBackend.Domain.Exceptions.BudgetExceptions;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.UpdateBudget
{
    public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, Unit>
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IMapper _mapper;

        public UpdateBudgetCommandHandler(IBudgetRepository budgetRepository,IMapper mapper)
        {
            _budgetRepository = budgetRepository;
            _mapper = mapper;
        }   
        public async Task<Unit> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
        {
            var mappedBudget = _mapper.Map<Budget>(request.UpdateBudgetDto);
            var budget = await _budgetRepository.GetByIdAsync(mappedBudget.Id, cancellationToken);

            if (budget == null) throw new BudgetNotFoundException(mappedBudget.Id.Id);

            budget.UpdateTitle(request.UpdateBudgetDto.Title);
            budget.UpdateTotalAmount(request.UpdateBudgetDto.TotalAmount);
           

            await _budgetRepository.UpdateAsync(budget, cancellationToken);

            return Unit.Value;
        }
    }
}
