namespace Buddy.Application.Dtos.Interview
{
    public class InterviewQuestionResult
    {
        public string QuestionText { get; set; } = string.Empty;
        public string? CodeSnippet { get; set; }
    }

    public class InterviewQuestionsRoot
    {
        public List<InterviewQuestionResult> Questions { get; set; } = new List<InterviewQuestionResult>();
    }
}
