namespace BudgetAppBackend.Application.Models.PlaidModels;
public record LinkTokenResponse(string LinkToken, string RequestId, DateTime Expiration);
