using MediatR;

namespace Buddy.Application.Features.Interview.GetReport
{
    public class GetInterviewReportQuery : IRequest<GetInterviewReportResponse>
    {
        public int SessionId { get; set; }
    }
}
