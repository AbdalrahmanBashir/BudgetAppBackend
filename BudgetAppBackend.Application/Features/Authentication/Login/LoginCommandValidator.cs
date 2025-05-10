using BudgetAppBackend.Application.Contracts;
using FluentValidation;

namespace BudgetAppBackend.Application.Features.Authentication.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.LogUser.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Must(email => email == email.ToLower()).WithMessage("Email must be in lowercase.")
                .EmailAddress().WithMessage("Invalid email format. Please enter a valid email.");

            RuleFor(x => x.LogUser.Password)
               .NotEmpty().WithMessage("Password is required.")
               .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
               .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter (A-Z).")
               .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter (a-z).")
               .Matches(@"\d").WithMessage("Password must contain at least one number (0-9).")
               .Matches(@"[@$!%*?&]").WithMessage("Password must contain at least one special character (@, $, !, %, *, ?, &).");
        }
    }
}
