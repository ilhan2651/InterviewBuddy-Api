using System.Collections.Generic;

namespace Buddy.Application.Features.Interview.GetReport
{
    public class GetInterviewReportResponse
    {
        public int OverallScore { get; set; }
        public int TechnicalScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ConfidenceScore { get; set; }
        public List<QuestionAnswerDto> QuestionAnswers { get; set; } = new List<QuestionAnswerDto>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public class QuestionAnswerDto
    {
        public string Question { get; set; } = string.Empty;
        public string? CodeSnippet { get; set; }
        public string UserAnswer { get; set; } = string.Empty;
        public string AiFeedback { get; set; } = string.Empty;
        public int Score { get; set; }
        public int? VideoScore { get; set; }
        public string? VideoFeedback { get; set; }
        public int? AudioScore { get; set; }
        public string? AudioFeedback { get; set; }
    }
}
