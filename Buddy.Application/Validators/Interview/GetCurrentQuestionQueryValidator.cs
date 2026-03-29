using Buddy.Application.Features.Interview.GetCurrentQuestion;
using FluentValidation;

namespace Buddy.Application.Validators.Interview
{
    public class GetCurrentQuestionQueryValidator : AbstractValidator<GetCurrentQuestionQuery>
    {
        public GetCurrentQuestionQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId zorunludur.")
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.");

            RuleFor(x => x.TargetQuestionNumber)
                .GreaterThan(0).WithMessage("Hedef soru numarası 0'dan büyük olmalıdır.")
                .When(x => x.TargetQuestionNumber.HasValue);
        }
    }
}
