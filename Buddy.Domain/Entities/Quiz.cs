using Buddy.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Buddy.Domain.Entities
{
    public class Quiz : BaseEntity
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public string Topic { get; set; } = string.Empty;
        public DifficultyLevel Difficulty { get; set; }
        public int QuestionCount { get; set; }
        public QuizStatus Status { get; set; } = QuizStatus.InProgress;
        
        public double? TotalScore { get; set; }
        public string? FeedbackSummary { get; set; }
        
        public DateTime QuestionsGeneratedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
