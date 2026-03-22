namespace Buddy.Application.Features.Quiz.CompleteQuiz
{
    public class QuestionResult
    {
        public int QuestionNumber { get; set; }
        public string Question { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
}
