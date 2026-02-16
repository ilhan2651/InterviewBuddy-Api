 using System;

namespace Buddy.Domain.Entities
{
    public class QuizAnswer : BaseEntity
    {
        public int QuizQuestionId { get; set; }
        public QuizQuestion QuizQuestion { get; set; } = null!;

        public string UserAnswer { get; set; } = string.Empty;
        public string? UserAudioPath { get; set; }
        
        public double? Score { get; set; }
        public string? Feedback { get; set; }
        
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
        public DateTime? EvaluatedAt { get; set; }
    }
}
