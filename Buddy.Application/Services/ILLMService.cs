using Buddy.Application.Dtos.Quiz;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface ILLMService
    {
        Task<string> TranscribeAudioAsync(Stream audioStream);
        Task<string> GenerateChatResponseAsync(string userMessage, List<Message> history);
        Task<Stream> TextToSpeechAsync(string text);
        Task<List<QuizQuestionDto>> GenerateQuizQuestionsAsync(string topic, DifficultyLevel difficulty, int count);
        Task<QuizEvaluationDto> EvaluateQuizAsync(List<QuizEvaluationInput> inputs);

        // Interview Methods
        Task<List<string>> GenerateInterviewQuestionsAsync(string jobTitle, InterviewLevel level, InterviewQuestionType type, int count);
        Task<AssessmentResult> EvaluateInterviewAnswerAsync(string question, string answer, string jobTitle, InterviewLevel level);
        Task<string> GenerateFinalFeedbackAsync(string jobTitle, InterviewLevel level, List<InterviewQuestion> questionsAndAnswers);
    }

    public class AssessmentResult
    {
        public string Feedback { get; set; } = string.Empty;
        public bool RequiresFollowUp { get; set; }
        public string? FollowUpQuestion { get; set; }
    }
}
