using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using BudgetAppBackend.Infrastructure.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Infrastructure.EventHandlers
{
    public class PlaidTransactionCreatedEventHandler : INotificationHandler<PlaidTransactionCreatedEvent>
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<TransactionCreatedEventHandler> _logger;

        public PlaidTransactionCreatedEventHandler(
            IBudgetRepository budgetRepository,
            IHubContext<NotificationHub> hubContext,
            ILogger<TransactionCreatedEventHandler> logger)
        {
            _budgetRepository = budgetRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Handle(PlaidTransactionCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var userId = UserId.Create(notification.UserId);
                var budget = await _budgetRepository.GetByCategoryAsync(notification.Category, userId, notification.Date, cancellationToken);

                if (budget is not null)
                {
                    budget.ApplyTransaction(notification.Amount);
                    await _budgetRepository.UpdateAsync(budget, cancellationToken);
                }
                _logger.LogInformation($"Attempting to send transaction notification to user {notification.UserId}");
                await _hubContext.Clients
                       .User(notification.UserId.ToString())
                       .SendAsync("ReceiveNewTransaction",
                           notification.Date,
                           notification.Category,
                           notification.normalizedAmount,
                           notification.Name ?? "Unknown",
                           cancellationToken);

                _logger.LogInformation($"Successfully sent transaction notification to user {notification.UserId}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling TransactionCreatedEvent for user {notification.UserId}");
                throw;
            }
        }
    }
}
