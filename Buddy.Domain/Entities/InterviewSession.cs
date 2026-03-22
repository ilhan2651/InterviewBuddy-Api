using Buddy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buddy.Domain.Entities
{
    public class InterviewSession : BaseEntity
    {
        public string Profession { get; set; } = string.Empty;
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
        public string Role { get; set; } = string.Empty; // e.g., ".NET Developer"
        public InterviewLevel Level { get; set; }
        public string Language { get; set; } = "Turkish"; // Preferred Language
        
        public int UserId { get; set; } // Foreign Key
        public User User { get; set; } = null!; // Navigation
        
        public string SessionId { get; set; } = string.Empty; // Conversation Session Id
        
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        // Navigation Properties
        public ICollection<InterviewQuestion> Questions { get; set; } = new List<InterviewQuestion>();

        public int? CommunicationScore { get; set; }
        public string? CommunicationFeedback { get; set; }
        public int? ConfidenceScore { get; set; }
        public string? ConfidenceFeedback { get; set; }

        public string? FinalFeedback { get; set; }
        public int? OverallScore { get; set; }

        public bool IsCompleted => CompletedAt.HasValue;
    }
}
