using FluentValidation;

namespace BudgetAppBackend.Application.Features.Authentication.SendVerificationCode
{
    public class SendVerificationCodeCommandValidator : AbstractValidator<SendVerificationCodeCommand>
    {
        public SendVerificationCodeCommandValidator()
        {
            RuleFor(x => x.SendVerificationCodeDto.Email)
               .NotEmpty().WithMessage("Email is required. Please enter your email address.")
               .EmailAddress().WithMessage("Invalid email format. Please enter a valid email like example@domain.com.")
               .Must(email => email == email.ToLower()).WithMessage("Email must be in lowercase.");
        }
    }
}
