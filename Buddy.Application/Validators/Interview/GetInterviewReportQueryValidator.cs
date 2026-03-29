using Buddy.Application.Features.Interview.GetReport;
using FluentValidation;

namespace Buddy.Application.Validators.Interview
{
    public class GetInterviewReportQueryValidator : AbstractValidator<GetInterviewReportQuery>
    {
        public GetInterviewReportQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId zorunludur.")
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.");
        }
    }
}
