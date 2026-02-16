namespace Buddy.Application.Features.Interview.GetCurrentQuestion
{
    public class GetCurrentQuestionResponse
    {
        public string QuestionText { get; set; } = string.Empty;
        public int QuestionNumber { get; set; }
        public string AudioUrl { get; set; } = string.Empty;
    }
}
