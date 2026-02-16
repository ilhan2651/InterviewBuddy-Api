using Buddy.Application.Features.Quiz.StartQuiz;
using FluentValidation;

namespace Buddy.Application.Validators.Quiz
{
    public class StartQuizCommandValidator : AbstractValidator<StartQuizCommand>
    {
        public StartQuizCommandValidator()
        {
            RuleFor(x => x.Topic)
                .NotEmpty().WithMessage("Konu alanı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Konu alanı en fazla 100 karakter olabilir.");

            RuleFor(x => x.QuestionCount)
                .InclusiveBetween(5, 20).WithMessage("Soru sayısı 5 ile 20 arasında olmalıdır.");

            RuleFor(x => x.Difficulty)
                .IsInEnum().WithMessage("Geçersiz zorluk seviyesi.");
        }
    }
}
