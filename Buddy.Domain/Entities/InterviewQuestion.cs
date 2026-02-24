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
        
        // Visual Question Enhancements
        public string? ImageUrl { get; set; }
        public string? CodeSnippet { get; set; }

        // Self-referencing relationship for follow-up questions
        public int? ParentId { get; set; }
        public InterviewQuestion? Parent { get; set; }
        public ICollection<InterviewQuestion> FollowUpQuestions { get; set; } = new List<InterviewQuestion>();

        // Navigation
        public InterviewAnswer? Answer { get; set; }
    }
}
