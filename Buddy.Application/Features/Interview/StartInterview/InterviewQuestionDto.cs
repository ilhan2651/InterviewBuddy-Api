using Buddy.Domain.Enums;

namespace Buddy.Application.Features.Interview.StartInterview
{
    public class InterviewQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public string DisplayNumber { get; set; } = string.Empty;
        public InterviewQuestionType Type { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? CodeSnippet { get; set; }
    }
}
