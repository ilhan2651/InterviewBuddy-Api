using System;

namespace Buddy.Domain.Entities
{
    public class QuizQuestion : BaseEntity
    {
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? ExpectedKeywords { get; set; } // Store as JSON string
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public QuizAnswer? Answer { get; set; }
    }
}
