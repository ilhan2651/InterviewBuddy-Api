using Buddy.Domain.Enums;
using MediatR;
using System.Collections.Generic;

namespace Buddy.Application.Features.Interview.StartInterview
{
    public class StartInterviewCommand : IRequest<StartInterviewResponse>
    {
        public string JobTitle { get; set; } = string.Empty;
        public InterviewLevel Level { get; set; }
        public string? SessionId { get; set; } // Optional, can create new if null
    }

    public class StartInterviewResponse
    {
        public int InterviewSessionId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public InterviewQuestionDto FirstQuestion { get; set; } = null!;
        public int TotalQuestions { get; set; }
    }

    public class InterviewQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public InterviewQuestionType Type { get; set; }
    }
}
