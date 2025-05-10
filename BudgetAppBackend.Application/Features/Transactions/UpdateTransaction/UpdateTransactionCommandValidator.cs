using FluentValidation;

namespace BudgetAppBackend.Application.Features.Transactions.UpdateTransaction
{
    public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
    {
        public UpdateTransactionCommandValidator()
        {
            RuleFor(x => x.UpdateTransactionDto.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");

            RuleFor(x => x.UpdateTransactionDto.TransactionId)
                .NotEmpty().WithMessage("Transaction ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Transaction ID must be a valid GUID.");

            RuleFor(x => x.UpdateTransactionDto.Amount)
                .GreaterThan(0).WithMessage("Transaction amount must be greater than zero.");

            RuleFor(x => x.UpdateTransactionDto.TransactionDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future.");
        }
    }
}
