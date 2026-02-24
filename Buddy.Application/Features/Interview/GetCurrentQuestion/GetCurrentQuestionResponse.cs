namespace Buddy.Application.Features.Interview.GetCurrentQuestion
{
    public class GetCurrentQuestionResponse
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int QuestionNumber { get; set; }
        public int TotalQuestions { get; set; }
        public string DisplayNumber { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        
        // Visual Question Enhancements
        public string? ImageUrl { get; set; }
        public string? CodeSnippet { get; set; }
    }
}
