using BudgetAppBackend.Application.Models.PlaidModels;
using MediatR;

namespace BudgetAppBackend.Application.Features.Plaid.ExchangePublicToken;

public record ExchangePublicTokenCommand(
        string PublicToken,
        Guid UserId,
        LinkSuccessMetadata? Metadata = null) : IRequest<LinkSuccessResult>;

public record LinkSuccessMetadata(
        string InstitutionId,
        string InstitutionName,
        List<LinkAccount> Accounts,
        string? LinkSessionId = null);


public record LinkAccount(
       string Id,
       string Name,
       string Mask,
       string Type,
       string? Subtype);

public record LinkSuccessResult(
        bool Success,
        string? AccessToken = null,
        string? ItemId = null,
        bool IsDuplicate = false,
        string? DuplicateAccessToken = null,
        string? Error = null);