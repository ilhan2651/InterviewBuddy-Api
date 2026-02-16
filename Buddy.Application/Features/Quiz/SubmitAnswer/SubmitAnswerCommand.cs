using MediatR;
using System.IO;

namespace Buddy.Application.Features.Quiz.SubmitAnswer
{
    public record SubmitAnswerCommand(
        int QuizQuestionId,
        string Answer,
        Stream? AudioStream
    ) : IRequest<SubmitAnswerResponse>;
}
