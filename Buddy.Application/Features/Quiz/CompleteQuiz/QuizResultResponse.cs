using System.Collections.Generic;

namespace Buddy.Application.Features.Quiz.CompleteQuiz
{
    public class QuizResultResponse
    {
        public double TotalScore { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<QuestionResult> Details { get; set; } = new List<QuestionResult>();
    }

    public class QuestionResult
    {
        public int QuestionNumber { get; set; }
        public string Question { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
}
