using MediatR;

namespace Buddy.Application.Features.Interview.GetReport
{
    public class GetInterviewReportQuery : IRequest<GetInterviewReportResponse>
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
