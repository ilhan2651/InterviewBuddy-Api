using Buddy.Domain.Enums;
using MediatR;

namespace Buddy.Application.Features.Quiz.StartQuiz
{
    public record StartQuizCommand(
        string? AnonymousId,
        string? SessionId,
        string Topic,
        DifficultyLevel Difficulty,
        int QuestionCount
    ) : IRequest<StartQuizResponse>;
}
