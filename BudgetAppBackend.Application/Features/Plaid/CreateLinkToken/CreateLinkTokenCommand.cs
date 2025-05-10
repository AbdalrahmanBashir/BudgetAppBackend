using BudgetAppBackend.Application.Models.PlaidModels;
using MediatR;

namespace BudgetAppBackend.Application.Features.Plaid.CreateLinkToken
{
    public record CreateLinkTokenCommand(string ClientUserId) : IRequest<LinkTokenResponse>;
}
