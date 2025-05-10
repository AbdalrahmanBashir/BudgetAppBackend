using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.UpdateBudget
{
    public class UpdateBudgetCommand : IRequest<Unit>
    {
        public UpdateBudgetDto UpdateBudgetDto { get; set; }
    }
}
