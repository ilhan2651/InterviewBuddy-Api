using Buddy.Application.Features.Admin.ReEvaluateAnswer;
using FluentValidation;

namespace Buddy.Application.Validators.Admin
{
    public class ReEvaluateAnswerCommandValidator : AbstractValidator<ReEvaluateAnswerCommand>
    {
        public ReEvaluateAnswerCommandValidator()
        {
            RuleFor(x => x.AnswerId)
                .GreaterThan(0).WithMessage("Geçerli bir cevap ID'si belirtilmelidir.");

            RuleFor(x => x.UpdatedAnswerText)
                .NotEmpty().WithMessage("Güncellenmiş cevap boş olamaz.")
                .MaximumLength(8000).WithMessage("Güncellenmiş cevap en fazla 8000 karakter olabilir.")
                .When(x => x.UpdatedAnswerText is not null);
        }
    }
}
