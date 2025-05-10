using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.DeleteAccount
{
    public class DeleteAccountCommand : IRequest<bool>
    {
        public string? Email { get; set; }
    }
}
