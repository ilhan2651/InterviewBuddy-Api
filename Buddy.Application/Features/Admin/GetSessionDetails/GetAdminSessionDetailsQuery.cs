using MediatR;
using System.Collections.Generic;

namespace Buddy.Application.Features.Admin.GetSessionDetails
{
    public class GetAdminSessionDetailsQuery : IRequest<AdminSessionDetailsDto>
    {
        public int SessionId { get; set; }
    }

    public class AdminSessionDetailsDto
    {
        public int SessionId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public List<AdminQuestionDto> Questions { get; set; } = new List<AdminQuestionDto>();
    }

    public class AdminQuestionDto
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public AdminAnswerDto? Answer { get; set; }
    }

    public class AdminAnswerDto
    {
        public int AnswerId { get; set; }
        public string UserAnswerText { get; set; } = string.Empty;
        public string AIAnalysis { get; set; } = string.Empty;
        public int Score { get; set; }
        public int? VideoScore { get; set; }
        public string? VideoFeedback { get; set; }
        public int? AudioScore { get; set; }
        public string? AudioFeedback { get; set; }
        public string AnsweredAt { get; set; } = string.Empty;
    }
}
