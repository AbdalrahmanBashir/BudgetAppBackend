using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.EventHandlers
{
    public class TransactionUpdatedEventHandler : INotificationHandler<TransactionUpdatedEvent>
    {
        private readonly IBudgetRepository _budgetRepository;
        public TransactionUpdatedEventHandler(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }
        public async Task Handle(TransactionUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(notification.UserId);
            var budget = await _budgetRepository.GetByCategoryAsync(notification.category, userId,notification.Date, cancellationToken);

            if (budget is not null)
            {

                budget.RollbackTransaction(notification.OldAmount);

                budget.ApplyTransaction(notification.NewAmount);

                await _budgetRepository.UpdateAsync(budget, cancellationToken);
            }
        }
    }
}
