  using FluentValidation;

namespace BudgetAppBackend.Application.Features.Transactions.CreateTransaction
{
    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        public CreateTransactionCommandValidator() { 


            RuleFor(x => x.createTransactionDto.Amount)
                .NotEmpty().WithMessage("Amount is required. Please enter the amount.");
            RuleFor(x => x.createTransactionDto.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero. Please enter a valid amount.");
            RuleFor(x => x.createTransactionDto.TransactionDate)
                .NotEmpty().WithMessage("Transaction date is required. Please enter the transaction date.");
            RuleFor(x => x.createTransactionDto.TransactionDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future. Please enter a valid date.");
            RuleFor(x => x.createTransactionDto.UserId)
                .NotEmpty().WithMessage("User ID is required. Please enter your user ID.")
                .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID. Please enter a valid user ID.");
            RuleFor(x => x.createTransactionDto.transactionType)
                .NotEmpty().WithMessage("Transaction type is required. Please specify the type of transaction (e.g., Income, Expense).")
                .Must(x => x == "Income" || x == "Expense").WithMessage("Transaction type must be either 'Income' or 'Expense'. Please enter a valid transaction type.");
            RuleFor(x => x.createTransactionDto.Categories)
                .NotEmpty().WithMessage("At least one category is required. Please select a category for the transaction.")
                .Must(categories => categories.Count > 0).WithMessage("At least one category must be selected. Please select a valid category.")
                .Must(categories => categories.All(c => !string.IsNullOrWhiteSpace(c))).WithMessage("Categories cannot be empty. Please enter valid categories.");
            RuleFor(x => x.createTransactionDto.payee)
                .NotEmpty().WithMessage("Payee is required. Please enter the name of the payee.")
                .MaximumLength(100).WithMessage("Payee name cannot exceed 100 characters. Please enter a shorter name.");
            RuleFor(x => x.createTransactionDto.payee)
                .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Payee name can only contain alphanumeric characters and spaces. Please enter a valid payee name.");

        }
    }
}
