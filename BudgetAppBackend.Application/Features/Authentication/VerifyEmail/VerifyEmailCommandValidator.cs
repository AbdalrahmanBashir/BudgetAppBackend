using FluentValidation;

namespace BudgetAppBackend.Application.Features.Authentication.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.VerifyEmailDto.Email)
               .NotEmpty().WithMessage("Email is required. Please enter your email address.")
               .EmailAddress().WithMessage("Invalid email format. Please enter a valid email like example@domain.com.")
               .Must(email => email == email.ToLower()).WithMessage("Email must be in lowercase.");
        }
    }
}
