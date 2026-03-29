using Buddy.Application.Features.Admin.GetSessions;
using FluentValidation;

namespace Buddy.Application.Validators.Admin
{
    public class GetAdminSessionsQueryValidator : AbstractValidator<GetAdminSessionsQuery>
    {
        public GetAdminSessionsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Geçerli bir kullanıcı ID'si belirtilmelidir.");
        }
    }
}
