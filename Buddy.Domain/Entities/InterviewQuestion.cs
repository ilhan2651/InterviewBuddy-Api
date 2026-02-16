using Buddy.Domain.Enums;
using System.Collections.Generic;

namespace Buddy.Domain.Entities
{
    public class InterviewQuestion : BaseEntity
    {
        public int InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; } = null!;

        public string QuestionText { get; set; } = string.Empty;
        public InterviewQuestionType Type { get; set; }
        public int Order { get; set; }
        public string? AudioUrl { get; set; } // AI sesli soru dosyası

        // Navigation
        public InterviewAnswer? Answer { get; set; }
    }
}
