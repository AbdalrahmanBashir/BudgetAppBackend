using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.BudgetAggregate.ValueObjects;
using BudgetAppBackend.Domain.Exceptions.BudgetExceptions;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.DeleteBudget
{
    public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Unit>
    {
        private readonly IBudgetRepository _budgetRepository;

        public DeleteBudgetCommandHandler(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }
        public async Task<Unit> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
        {
            var budgetId = BudgetId.Create(request.DeleteBudgetDto.BudgetId);
            var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
            if (budget == null) {
                throw new BudgetNotFoundException(budgetId.Id);
            }
            if (budget.UserId.Id != request.DeleteBudgetDto.UserId) {
                throw new UnauthorizedAccessException("You are not authorized to delete this budget.");
            }

            await _budgetRepository.DeleteAsync(budget, cancellationToken);

            return Unit.Value;
        }
    }
}
