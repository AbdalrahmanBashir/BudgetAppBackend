namespace BudgetAppBackend.Application.Models.PlaidModels;

public record AccountsResponse(IReadOnlyList<PlaidAccount> Accounts, string RequestId);

public record PlaidAccount(
    string AccountId,
    string Name,
    string Type,
    string SubType,
    decimal CurrentBalance,
    decimal AvailableBalance);


