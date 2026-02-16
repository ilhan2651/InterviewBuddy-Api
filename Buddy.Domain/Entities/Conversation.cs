using System;
using System.Collections.Generic;

namespace Buddy.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public int? UserId { get; set; }
        public User? User { get; set; }

        public string SessionId { get; set; } = string.Empty;
        public string? AnonymousId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<User> Participants { get; set; } = new List<User>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
