using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.EventHandlers
{
    public sealed class PlaidTransactionEventHandler :
    INotificationHandler<PlaidTransactionModifiedDomainEvent>,
    INotificationHandler<PlaidTransactionRemovedDomainEvent>
    {
        private readonly IBudgetRepository _budgetRepository;
        public PlaidTransactionEventHandler(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }
       

        public async Task Handle(PlaidTransactionModifiedDomainEvent notification, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(notification.UserId);
            var budget = await _budgetRepository.GetByCategoryAsync(notification.Category, userId, notification.Date, cancellationToken);

            if (budget is not null)
            {

                budget.RollbackTransaction(notification.oldAmount);

                budget.ApplyTransaction(notification.Amount);

                await _budgetRepository.UpdateAsync(budget, cancellationToken);
            }
        }

        public async Task Handle(PlaidTransactionRemovedDomainEvent notification, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(notification.UserId);
            var budget = await _budgetRepository.GetByCategoryAsync(notification.Category, userId, notification.Date, cancellationToken);

            if (budget is not null)
            {
                budget.RollbackTransaction(notification.Amount);

                await _budgetRepository.UpdateAsync(budget, cancellationToken);
            }
        }
    }
}
