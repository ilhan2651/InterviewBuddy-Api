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
        public int? Score { get; set; } // 0-100 text score
        
        // Video Analysis
        public int? VideoScore { get; set; }
        public string? VideoFeedback { get; set; }

        // Audio Analysis
        public int? AudioScore { get; set; }
        public string? AudioFeedback { get; set; }

        public int FollowUpCount { get; set; } = 0;
    }
}
