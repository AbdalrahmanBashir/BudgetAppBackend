namespace BudgetAppBackend.Infrastructure.SignalR
{
    public interface INotificationClient
    {
        Task ReceiveNotification(string message);
        Task ReceiveNewTransaction(DateTime transactionDate, string category, decimal amount, string name);
    }
}