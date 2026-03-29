using Buddy.Application.Features.Interview.SubmitAnswer;
using FluentValidation;

namespace Buddy.Application.Validators.Interview
{
    public class SubmitInterviewAnswerCommandValidator : AbstractValidator<SubmitInterviewAnswerCommand>
    {
        public SubmitInterviewAnswerCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId zorunludur.")
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.");

            RuleFor(x => x.QuestionId)
                .GreaterThan(0).WithMessage("Geçerli bir soru ID'si gönderilmelidir.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.AnswerText) || !string.IsNullOrWhiteSpace(x.AudioPath))
                .WithMessage("Cevap metni veya ses yolu gönderilmelidir.");

            RuleFor(x => x.AnswerText)
                .MaximumLength(8000).WithMessage("Cevap metni en fazla 8000 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.AnswerText));

            RuleFor(x => x.AudioPath)
                .MaximumLength(500).WithMessage("Ses yolu en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.AudioPath));
        }
    }
}
