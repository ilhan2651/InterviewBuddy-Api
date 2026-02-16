using Buddy.Application.Features.Quiz.SubmitAnswer;
using FluentValidation;

namespace Buddy.Application.Validators.Quiz
{
    public class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
    {
        public SubmitAnswerCommandValidator()
        {
            RuleFor(x => x.QuizQuestionId)
                .GreaterThan(0).WithMessage("Geçerli bir soru ID'si belirtilmelidir.");

            RuleFor(x => x.Answer)
                .NotEmpty()
                .When(x => x.AudioStream == null)
                .WithMessage("Cevap metni veya ses dosyası gönderilmelidir.");
        }
    }
}
