using BudgetAppBackend.Application.Models.PlaidModels;
using MediatR;

namespace BudgetAppBackend.Application.Features.Plaid.GetAccounts;
public record GetAccountsQuery(string AccessToken) : IRequest<AccountsResponse>;
