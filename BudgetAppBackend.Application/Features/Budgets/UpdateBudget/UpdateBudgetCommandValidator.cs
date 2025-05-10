using FluentValidation;

namespace BudgetAppBackend.Application.Features.Budgets.UpdateBudget
{
    public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
    {
        public UpdateBudgetCommandValidator()
        {
            RuleFor(x => x.UpdateBudgetDto)
                .NotNull().WithMessage("UpdateBudgetDto is required.");

            When(x => x.UpdateBudgetDto != null, () =>
            {
                RuleFor(x => x.UpdateBudgetDto.BudgetId)
                    .NotEmpty().WithMessage("BudgetId is required.");

                RuleFor(x => x.UpdateBudgetDto.Title)
                    .NotEmpty().WithMessage("Title is required.")
                    .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

                RuleFor(x => x.UpdateBudgetDto.TotalAmount)
                    .GreaterThanOrEqualTo(0).WithMessage("TotalAmount must be non-negative.");
            });
        }
    }
}
