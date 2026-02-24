using Buddy.Application.Dtos.Quiz;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface IQuizLLMService
    {
        Task<string> TranscribeAudioAsync(Stream audioStream);
        Task<string> GenerateChatResponseAsync(string userMessage, List<Message> history);
        Task<Stream> TextToSpeechAsync(string text);
        Task<List<QuizQuestionDto>> GenerateQuizQuestionsAsync(string topic, DifficultyLevel difficulty, int count);
        Task<QuizEvaluationDto> EvaluateQuizAsync(List<QuizEvaluationInput> inputs);
    }
}
