using Buddy.Domain.Enums;
using MediatR;

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
}
