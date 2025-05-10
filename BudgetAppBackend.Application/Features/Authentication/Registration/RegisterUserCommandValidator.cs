using BudgetAppBackend.Application.Contracts;
using FluentValidation;

namespace BudgetAppBackend.Application.Features.Authentication.Registration
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator(IAuthRepository _authRepository)
        {
            RuleFor(x => x.AddUser.FirstName)
                .NotEmpty().WithMessage("First name is required. Please enter your first name.");

            RuleFor(x => x.AddUser.LastName)
                .NotEmpty().WithMessage("Last name is required. Please enter your last name.");

            RuleFor(x => x.AddUser.Email)
                .NotEmpty().WithMessage("Email is required. Please enter your email address.")
                .EmailAddress().WithMessage("Invalid email format. Please enter a valid email like example@domain.com.")
                .Must(email => string.Equals(email, email.ToLower(), StringComparison.Ordinal))
                .WithMessage("Email must be in lowercase.")
                .MustAsync(async (email, cancellationToken) =>
                    await _authRepository.GetUserByEmailAsync(email, cancellationToken) == null)
                .WithMessage("This email address is already registered. Please use a different email or log in.");


            RuleFor(x => x.AddUser.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter (A-Z).")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter (a-z).")
                .Matches(@"\d").WithMessage("Password must contain at least one number (0-9).")
                .Matches(@"[@$!%*?&]").WithMessage("Password must contain at least one special character (@, $, !, %, *, ?, &).");

            RuleFor(x => x.AddUser.Confirmpassword)
                .NotEmpty().WithMessage("Confirm password is required. Please re-enter your password.")
                .Equal(x => x.AddUser.Password)
                .WithMessage("The confirmation password does not match the entered password. Please ensure both fields contain the same password.");

        }
    }
}
