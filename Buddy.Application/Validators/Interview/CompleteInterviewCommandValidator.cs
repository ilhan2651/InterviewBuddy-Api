using Buddy.Application.Features.Interview.CompleteInterview;
using FluentValidation;

namespace Buddy.Application.Validators.Interview
{
    public class CompleteInterviewCommandValidator : AbstractValidator<CompleteInterviewCommand>
    {
        public CompleteInterviewCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId zorunludur.")
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.");
        }
    }
}
