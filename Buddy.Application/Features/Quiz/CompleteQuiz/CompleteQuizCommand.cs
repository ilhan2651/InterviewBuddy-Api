using MediatR;

namespace Buddy.Application.Features.Quiz.CompleteQuiz
{
    public record CompleteQuizCommand(int QuizId) : IRequest<QuizResultResponse>;
}
