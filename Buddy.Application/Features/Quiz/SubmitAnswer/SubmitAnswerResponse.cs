using Buddy.Application.Dtos.Quiz;

namespace Buddy.Application.Features.Quiz.SubmitAnswer
{
    public class SubmitAnswerResponse
    {
        public int QuizId { get; set; }
        public bool HasMore { get; set; }
        public QuizQuestionDto? NextQuestion { get; set; }
        public string? CompletionMessage { get; set; }
    }
}
