using MediatR;

namespace Buddy.Application.Features.Interview.CompleteInterview
{
    public class CompleteInterviewCommand : IRequest<CompleteInterviewResponse>
    {
        public string SessionId { get; set; } = string.Empty;
    }

    public class CompleteInterviewResponse
    {
        public int TotalScore { get; set; }
        public string FinalFeedbackReport { get; set; } = string.Empty;
    }
}
