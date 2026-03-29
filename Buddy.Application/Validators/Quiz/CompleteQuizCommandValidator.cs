using Buddy.Application.Features.Quiz.CompleteQuiz;
using FluentValidation;

namespace Buddy.Application.Validators.Quiz
{
    public class CompleteQuizCommandValidator : AbstractValidator<CompleteQuizCommand>
    {
        public CompleteQuizCommandValidator()
        {
            RuleFor(x => x.QuizId)
                .GreaterThan(0).WithMessage("Geçerli bir quiz ID'si belirtilmelidir.");
        }
    }
}
