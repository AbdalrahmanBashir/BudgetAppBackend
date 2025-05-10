using BudgetAppBackend.Application.Models.PlaidModels;
using MediatR;

namespace BudgetAppBackend.Application.Features.Plaid.SyncTransactions;
public record SyncTransactionsCommand(
    Guid userId,
    string AccessToken,
    string? Cursor = null,
    int Count = 5) : IRequest<TransactionsSyncResponse>;