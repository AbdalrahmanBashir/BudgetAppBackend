using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BudgetAppBackend.Infrastructure.SignalR
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                //await Clients.User(userId).SendAsync("ReceiveNotification", $"🔔 Welcome, User {userId}!");
                _logger.LogInformation($"User {userId} connected to SignalR Hub.");
            }
            else
            {
                _logger.LogWarning("⚠️ SignalR: A user connected but `UserIdentifier` is NULL.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userId} disconnected from SignalR Hub.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public async Task SendNotification(Guid userId, string message)
        {
            await Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
        }

        [Authorize]
        public async Task NotifyNewTransaction(Guid userId, decimal amount, string category, DateTime transactionDate, string name)
        {
            var message = $"💰 New transaction: {amount:C} in {category} on {transactionDate:MMMM d, yyyy}";
            _logger.LogInformation($"📢 Sending transaction notification to User {userId}: {message}");

            await Clients.User(userId.ToString()).SendAsync("ReceiveNewTransaction", transactionDate, category, amount, name);
        }
    }

}
