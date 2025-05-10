using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using BudgetAppBackend.Infrastructure.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Infrastructure.EventHandlers
{
    public class TransactionCreatedEventHandler : INotificationHandler<TransactionCreatedEvent>
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<TransactionCreatedEventHandler> _logger;

        public TransactionCreatedEventHandler(
            IBudgetRepository budgetRepository,
            IHubContext<NotificationHub> hubContext,
            ILogger<TransactionCreatedEventHandler> logger)
        {
            _budgetRepository = budgetRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Handle(TransactionCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Starting to handle TransactionCreatedEvent for user {notification.UserId}");

                var userId = UserId.Create(notification.UserId);
                var budget = await _budgetRepository.GetByCategoryAsync(notification.category, userId, notification.Date, cancellationToken);

                if (budget is not null && notification.Type.ToString() == "Expense")
                {
                    budget.ApplyTransaction(notification.amount);
                    await _budgetRepository.UpdateAsync(budget, cancellationToken);
                }

                _logger.LogInformation($"Attempting to send transaction notification to user {notification.UserId}");

                await _hubContext.Clients
                    .User(notification.UserId.ToString())
                    .SendAsync("ReceiveNewTransaction",
                        notification.Date,
                        notification.category,
                        notification.amount,
                        notification.payee ?? "Unknown",
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
