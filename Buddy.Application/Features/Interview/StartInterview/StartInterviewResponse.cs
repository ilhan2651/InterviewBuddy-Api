namespace Buddy.Application.Features.Interview.StartInterview
{
    public class StartInterviewResponse
    {
        public int InterviewSessionId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public InterviewQuestionDto FirstQuestion { get; set; } = null!;
        public int TotalQuestions { get; set; }
    }
}
