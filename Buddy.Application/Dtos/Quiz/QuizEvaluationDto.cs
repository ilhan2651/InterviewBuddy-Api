using System.Collections.Generic;

namespace Buddy.Application.Dtos.Quiz
{
    public class QuizEvaluationInput
    {
        public int QuestionNumber { get; set; }
        public string Question { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public List<string> ExpectedKeywords { get; set; } = new List<string>();
    }

    public class QuestionEvaluation
    {
        public int QuestionNumber { get; set; }
        public double Score { get; set; } // 0-10
        public string Feedback { get; set; } = string.Empty;
    }

    public class QuizEvaluationDto
    {
        public List<QuestionEvaluation> Evaluations { get; set; } = new List<QuestionEvaluation>();
        public double TotalScore { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
