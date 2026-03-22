using System.Collections.Generic;

namespace Buddy.Application.Features.Quiz.CompleteQuiz
{
    public class QuizResultResponse
    {
        public double TotalScore { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<QuestionResult> Details { get; set; } = new List<QuestionResult>();
    }
}
