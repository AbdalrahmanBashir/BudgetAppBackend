using FluentValidation;

namespace BudgetAppBackend.Application.Features.Budgets.CreateBudget
{
    public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
    {
        public CreateBudgetCommandValidator()
        {
            RuleFor(x => x.CreateBudgetDto)
                .NotNull()
                .WithMessage("CreateBudgetDto is required.");

            When(x => x.CreateBudgetDto != null, () =>
            {
                RuleFor(x => x.CreateBudgetDto.UserId)
                    .NotEmpty().WithMessage("UserId is required.");

                RuleFor(x => x.CreateBudgetDto.Title)
                    .NotEmpty().WithMessage("Title is required.")
                    .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

                RuleFor(x => x.CreateBudgetDto.TotalAmount)
                    .GreaterThan(0).WithMessage("TotalAmount must be greater than 0.");

                RuleFor(x => x.CreateBudgetDto.Category)
                    .NotEmpty().WithMessage("CategoryId is required.");

            });
        }
    }
}
