using Buddy.Domain.Enums;
using MediatR;
using System.Collections.Generic;

namespace Buddy.Application.Features.Interview.StartInterview
{
    public class StartInterviewCommand : IRequest<StartInterviewResponse>
    {
        public string Profession { get; set; } = string.Empty; // e.g., "Yazılım Geliştirme"
        public string JobTitle { get; set; } = string.Empty;    // e.g., ".NET Developer"
        public InterviewLevel Level { get; set; }
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
        public string Language { get; set; } = "Turkish"; // Default to Turkish
        public string? SessionId { get; set; } // Optional, can create new if null
    }

    public class StartInterviewResponse
    {
        public int InterviewSessionId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public InterviewQuestionDto FirstQuestion { get; set; } = null!;
        public int TotalQuestions { get; set; }
    }

    public class InterviewQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public string DisplayNumber { get; set; } = string.Empty;
        public InterviewQuestionType Type { get; set; }
        public string? AudioUrl { get; set; }
        
        // Visual Question Enhancements
        public string? ImageUrl { get; set; }
        public string? CodeSnippet { get; set; }
    }
}
