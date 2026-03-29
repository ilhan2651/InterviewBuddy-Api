using Buddy.Application.Features.Interview.StartInterview;
using FluentValidation;

namespace Buddy.Application.Validators.Interview
{
    public class StartInterviewCommandValidator : AbstractValidator<StartInterviewCommand>
    {
        public StartInterviewCommandValidator()
        {
            RuleFor(x => x.Profession)
                .NotEmpty().WithMessage("Meslek alanı zorunludur.")
                .MaximumLength(100).WithMessage("Meslek alanı en fazla 100 karakter olabilir.");

            RuleFor(x => x.JobTitle)
                .NotEmpty().WithMessage("Pozisyon alanı zorunludur.")
                .MaximumLength(100).WithMessage("Pozisyon alanı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Level)
                .IsInEnum().WithMessage("Geçersiz seviye bilgisi.");

            RuleFor(x => x.Difficulty)
                .IsInEnum().WithMessage("Geçersiz zorluk seviyesi.");

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Dil alanı zorunludur.")
                .MaximumLength(50).WithMessage("Dil alanı en fazla 50 karakter olabilir.");

            RuleFor(x => x.SessionId)
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.SessionId));
        }
    }
}
