using Buddy.Application.Features.Admin.GetSessionDetails;
using FluentValidation;

namespace Buddy.Application.Validators.Admin
{
    public class GetAdminSessionDetailsQueryValidator : AbstractValidator<GetAdminSessionDetailsQuery>
    {
        public GetAdminSessionDetailsQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0).WithMessage("Geçerli bir session ID'si belirtilmelidir.");
        }
    }
}
