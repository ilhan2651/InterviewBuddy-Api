using System.Collections.Generic;

namespace Buddy.Application.Features.Admin.GetSessionDetails
{
    public class AdminSessionDetailsDto
    {
        public int SessionId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public List<AdminQuestionDto> Questions { get; set; } = new List<AdminQuestionDto>();
    }
}
