using MediatR;

namespace Buddy.Application.Features.Admin.GetSessionDetails
{
    public class GetAdminSessionDetailsQuery : IRequest<AdminSessionDetailsDto>
    {
        public int SessionId { get; set; }
    }
}
