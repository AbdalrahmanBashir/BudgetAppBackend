using FluentValidation;

namespace BudgetAppBackend.Application.Features.Authentication.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.resetPassword.Email)
               .NotEmpty().WithMessage("Email is required. Please enter your email address.")
               .EmailAddress().WithMessage("Invalid email format. Please enter a valid email like example@domain.com.")
               .Must(email => email == email.ToLower()).WithMessage("Email must be in lowercase.");

            RuleFor(x => x.resetPassword.NewPassword)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter (A-Z).")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter (a-z).")
                .Matches(@"\d").WithMessage("Password must contain at least one number (0-9).")
                .Matches(@"[@$!%*?&]").WithMessage("Password must contain at least one special character (@, $, !, %, *, ?, &).");

            RuleFor(x => x.resetPassword.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm password is required. Please re-enter your password.")
                .Equal(x => x.resetPassword.NewPassword)
                .WithMessage("The confirmation password does not match the entered password. Please ensure both fields contain the same password.");
        }
    }
}
