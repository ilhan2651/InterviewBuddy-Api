using MediatR;

namespace Buddy.Application.Features.Interview.GetCurrentQuestion
{
    public class GetCurrentQuestionQuery : IRequest<GetCurrentQuestionResponse>
    {
        public int SessionId { get; set; }
    }
}
