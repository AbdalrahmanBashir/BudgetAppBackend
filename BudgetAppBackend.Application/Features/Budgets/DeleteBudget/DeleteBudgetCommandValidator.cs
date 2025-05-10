using FluentValidation;

namespace BudgetAppBackend.Application.Features.Budgets.DeleteBudget
{
    public class DeleteBudgetCommandValidator : AbstractValidator<DeleteBudgetCommand>
    {
        public DeleteBudgetCommandValidator()
        {
            RuleFor(x => x.DeleteBudgetDto)
                .NotNull()
                .WithMessage("DeleteBudgetDto is required.");

            When(x => x.DeleteBudgetDto != null, () =>
            {
                RuleFor(x => x.DeleteBudgetDto.BudgetId)
                    .NotEmpty()
                    .WithMessage("BudgetId is required.");

                RuleFor(x => x.DeleteBudgetDto.UserId)
                    .NotEmpty()
                    .WithMessage("UserId is required.");
            });
        }
    }
}
