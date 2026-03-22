namespace Buddy.Application.Features.Interview.GetReport
{
    public class QuestionAnswerDto
    {
        public string Question { get; set; } = string.Empty;
        public string? CodeSnippet { get; set; }
        public string UserAnswer { get; set; } = string.Empty;
        public string AiFeedback { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? IdealAnswerSummary { get; set; }
    }
}
