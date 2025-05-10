using BudgetAppBackend.Domain.PlaidTransactionAggregate;

namespace BudgetAppBackend.Application.Models.PlaidModels;
public record TransactionsResponse(IReadOnlyList<PlaidTransaction> Transactions, string RequestId);