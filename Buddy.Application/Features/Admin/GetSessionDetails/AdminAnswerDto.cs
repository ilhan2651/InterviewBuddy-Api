namespace Buddy.Application.Features.Admin.GetSessionDetails
{
    public class AdminAnswerDto
    {
        public int AnswerId { get; set; }
        public string UserAnswerText { get; set; } = string.Empty;
        public string AIAnalysis { get; set; } = string.Empty;
        public int Score { get; set; }
        public string AnsweredAt { get; set; } = string.Empty;
    }
}
