using FluentValidation;

namespace BudgetAppBackend.Application.Features.Transactions.DeleteTransaction
{
    public class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
    {
        public DeleteTransactionCommandValidator()
        {
            RuleFor(x => x.DeleteTransactionDto.TransactionId)
                .NotEmpty().WithMessage("Transaction ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Transaction ID must be a valid GUID.");

            RuleFor(x => x.DeleteTransactionDto.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");
        }
    }
}
