using BudgetAppBackend.Application.Models;

namespace BudgetAppBackend.Application.Service
{
    public interface IEmailService
    {
        Task<bool> SendEmail(EmailMessage email, CancellationToken cancellationToken);
    }
}
