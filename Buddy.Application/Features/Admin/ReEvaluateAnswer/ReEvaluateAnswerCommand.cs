using MediatR;

namespace Buddy.Application.Features.Admin.ReEvaluateAnswer
{
    public class ReEvaluateAnswerCommand : IRequest<ReEvaluateAnswerResponse>
    {
        public int AnswerId { get; set; }
        // Optional override to test a different text string through LLM for the same question
        public string? UpdatedAnswerText { get; set; } 
    }

    public class ReEvaluateAnswerResponse
    {
        public int AnswerId { get; set; }
        public string NewFeedback { get; set; } = string.Empty;
        public int NewScore { get; set; }
    }
}
