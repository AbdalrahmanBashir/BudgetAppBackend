using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.DeleteBudget
{
    public class DeleteBudgetCommand : IRequest<Unit>
    {
        public DeleteBudgetDto DeleteBudgetDto { get; set; }
    }
}
