using System.Collections.Generic;

namespace Buddy.Application.Features.Interview.GetReport
{
    public class GetInterviewReportResponse
    {
        public int OverallScore { get; set; }
        public int TechnicalScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ConfidenceScore { get; set; }
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> ImprovementAreas { get; set; } = new List<string>();
        public List<string> ImprovmentArea { get; set; } = new List<string>();
        public List<QuestionAnswerDto> QuestionAnswers { get; set; } = new List<QuestionAnswerDto>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}
