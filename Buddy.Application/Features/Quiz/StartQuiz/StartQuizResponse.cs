using Buddy.Application.Dtos.Quiz;

namespace Buddy.Application.Features.Quiz.StartQuiz
{
    public class StartQuizResponse
    {
        public int QuizId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public QuizQuestionDto? FirstQuestion { get; set; }
        public int TotalQuestions { get; set; }
    }
}
