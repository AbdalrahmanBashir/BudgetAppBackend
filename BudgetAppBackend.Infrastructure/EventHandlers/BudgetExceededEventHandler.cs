using BudgetAppBackend.Domain.DomainEvents;
using BudgetAppBackend.Infrastructure.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BudgetAppBackend.Infrastructure.EventHandlers
{
    public class BudgetExceededEventHandler : INotificationHandler<BudgetExceededEvent>
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public BudgetExceededEventHandler(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        
        public async Task Handle(BudgetExceededEvent notification, CancellationToken cancellationToken)
        {
            var message = $"⚠️ Budget Alert: Spent {notification.SpentAmount:C} in category {notification.category}, exceeding limit of {notification.BudgetLimit:C}.";

            await _hubContext.Clients
                .User(notification.UserId.ToString())
                .SendAsync("ReceiveNotification", message, cancellationToken);
        }
    }
}
