using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.GetSpendingPerCategory;

public sealed record GetSpendingPerCategoryQuery(Guid UserId) : IRequest<IEnumerable<CategoryTotalDto>>;