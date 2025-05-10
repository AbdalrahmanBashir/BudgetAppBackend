using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.EventHandlers
{
    public class TransactionDeletedEventHandler : INotificationHandler<TransactionDeletedEvent>
    {
        private readonly IBudgetRepository _budgetRepository;

        public TransactionDeletedEventHandler(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }
        public async Task Handle(TransactionDeletedEvent notification, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(notification.UserId);
            var budget = await _budgetRepository.GetByCategoryAsync(notification.category, userId, notification.Date,  cancellationToken);

            if (budget is not null )
            {
                budget.RollbackTransaction(notification.Amount);

                await _budgetRepository.UpdateAsync(budget, cancellationToken);
            }
        }
    }
}
