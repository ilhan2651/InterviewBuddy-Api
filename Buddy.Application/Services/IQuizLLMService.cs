using Buddy.Application.Dtos.Quiz;
using Buddy.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface IQuizLLMService
    {
        Task<List<QuizQuestionDto>> GenerateQuizQuestionsAsync(string topic, DifficultyLevel difficulty, int count);
        Task<QuizEvaluationDto> EvaluateQuizAsync(List<QuizEvaluationInput> inputs);
    }
}
