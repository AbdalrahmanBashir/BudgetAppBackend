using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.BudgetAggregate;
using BudgetAppBackend.Domain.Exceptions.BudgetExceptions;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.CreateBudget
{
    public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, Unit>
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _plaidTransactionRepository;

        public CreateBudgetCommandHandler(
            IBudgetRepository budgetRepository,
            ITransactionRepository transactionRepository,
            IPlaidTransactionRepository laidTransactionRepository)
        {
            _budgetRepository = budgetRepository;
            _transactionRepository = transactionRepository;
            _plaidTransactionRepository = laidTransactionRepository;
        }

        public async Task<Unit> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.CreateBudgetDto.UserId);
            var createdDate = DateTime.UtcNow;
           
            var existingBudgets = await _budgetRepository.GetBudgetsByUserIdAsync(userId);

            if (existingBudgets != null)
            {
                foreach(var existingBudget in existingBudgets)
                {
                    if (existingBudget.Category == request.CreateBudgetDto.Category) {
                        throw new BudgetAlreadyExistsException(userId.Id, request.CreateBudgetDto.Category);    
                    }
                }
            }

            var budget = Budget.Create(
                userId, 
                request.CreateBudgetDto.Title, 
                request.CreateBudgetDto.TotalAmount,
                request.CreateBudgetDto.Category, 
                createdDate
            );

            var pastTransactions = await _transactionRepository.GetTransactionsByUserAndCategoryAsync(userId, request.CreateBudgetDto.Category, createdDate, cancellationToken);
            var pastPlaidTransactions = await _plaidTransactionRepository.GetTransactionsByUserAndCategoryAsync(userId, request.CreateBudgetDto.Category, createdDate, cancellationToken);

            budget.ApplyPastTransactions(pastTransactions);
            budget.ApplyPastPlaidTransactions(pastPlaidTransactions);

            await _budgetRepository.AddAsync(budget, cancellationToken);

            return Unit.Value;
        }
    }
}
