using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.GetBudgetsByUser
{
    public class GetBudgetsByUserQuery : IRequest<List<BudgetDto>>
    {
        public Guid UserId { get; set; }
    }
}
