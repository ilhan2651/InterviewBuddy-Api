using Buddy.Application.Dtos.Interview;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface IInterviewLLMService
    {
        Task<List<InterviewQuestionResult>> GenerateInterviewQuestionsAsync(string profession, string jobTitle, InterviewLevel level, DifficultyLevel difficulty, InterviewQuestionType type, int count, string language, List<string>? previouslyAskedQuestions = null, CancellationToken cancellationToken = default);
        Task<AssessmentResult> EvaluateInterviewAnswerAsync(string question, string answer, string profession, string jobTitle, InterviewLevel level, DifficultyLevel difficulty, string language, CancellationToken cancellationToken = default);
        Task<AssessmentResult> EvaluateImageAsync(string base64Image, string language, CancellationToken cancellationToken = default);
        Task<AssessmentResult> EvaluateAudioToneAsync(Stream audioStream, string language, CancellationToken cancellationToken = default);
        Task<FollowUpResult> DecideFollowUpAsync(string question, string answer, string language, CancellationToken cancellationToken = default);
        Task<string> GenerateFinalFeedbackAsync(string profession, string jobTitle, InterviewLevel level, DifficultyLevel difficulty, List<InterviewQuestion> questionsAndAnswers, string language, CancellationToken cancellationToken = default);
    }
}
