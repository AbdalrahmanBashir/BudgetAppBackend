using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetDetailedDailyCashFlow;

public sealed record GetDetailedDailyCashFlowQuery(Guid UserId) : IRequest<IEnumerable<DetailedDailyCashFlowDto>>;
