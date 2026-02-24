using MediatR;

namespace Buddy.Application.Features.Interview.GetCurrentQuestion
{
    public class GetCurrentQuestionQuery : IRequest<GetCurrentQuestionResponse>
    {
        public string SessionId { get; set; } = string.Empty;
        public int? TargetQuestionNumber { get; set; }
    }
}
