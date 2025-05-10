using BudgetAppBackend.Application.Contracts;
using MediatR;
using Quartz;

namespace BudgetAppBackend.Infrastructure.Jobs
{
    public class ResetBudgetJob : IJob
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IMediator _mediator;

        public ResetBudgetJob(IBudgetRepository budgetRepository, IMediator mediator)
        {
            _budgetRepository = budgetRepository;
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Executing Budget Reset Job...");

            var budgets = await _budgetRepository.GetAllBudgetsAsync();

            foreach (var budget in budgets)
            {
                budget.ResetForNewMonth();
                await _budgetRepository.UpdateAsync(budget, CancellationToken.None);
            }

            Console.WriteLine("Budget Reset Job Completed.");
        }
    }
}
