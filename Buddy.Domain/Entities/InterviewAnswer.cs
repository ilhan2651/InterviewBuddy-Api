using System;

namespace Buddy.Domain.Entities
{
    public class InterviewAnswer : BaseEntity
    {
        public int InterviewQuestionId { get; set; }
        public InterviewQuestion InterviewQuestion { get; set; } = null!;

        public string? UserAnswerText { get; set; }
        public string? UserAudioPath { get; set; }
        
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        // Feedback
        public string? AIAnalysis { get; set; }
        public int? Score { get; set; } // 0-100 or 1-10
        public int FollowUpCount { get; set; } = 0;
    }
}
