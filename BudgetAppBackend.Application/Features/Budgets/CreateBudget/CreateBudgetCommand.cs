using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using MediatR;

namespace BudgetAppBackend.Application.Features.Budgets.CreateBudget
{
    public class CreateBudgetCommand : IRequest<Unit>
    {
        public CreateBudgetDto CreateBudgetDto { get; set; }
    }
}
