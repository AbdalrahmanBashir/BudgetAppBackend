namespace BudgetAppBackend.Application.Models.PlaidModels;

public record AccessTokenResponse(
    string AccessToken,
    string ItemId,
    string RequestId,
    bool IsDuplicate = false);
