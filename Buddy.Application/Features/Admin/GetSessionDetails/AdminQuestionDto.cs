namespace Buddy.Application.Features.Admin.GetSessionDetails
{
    public class AdminQuestionDto
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public AdminAnswerDto? Answer { get; set; }
    }
}
