using Buddy.Application.Features.User.GetRecentInterviews;
using MediatR;
using System.Collections.Generic;

namespace Buddy.Application.Features.Admin.GetSessions
{
    public class GetAdminSessionsQuery : IRequest<List<AdminSessionDto>>
    {
        public int UserId { get; set; }
    }

    public class AdminSessionDto
    {
        public int SessionId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string? CompletedAt { get; set; }
        public bool IsCompleted => CompletedAt != null;
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int OverallScore { get; set; }
    }
}
