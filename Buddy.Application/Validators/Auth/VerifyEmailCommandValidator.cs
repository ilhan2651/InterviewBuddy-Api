using Buddy.Application.Features.Auth.VerifyEmail;
using FluentValidation;

namespace Buddy.Application.Validators.Auth
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Doğrulama tokenı zorunludur.")
                .MaximumLength(200).WithMessage("Doğrulama tokenı geçersiz.");
        }
    }
}
