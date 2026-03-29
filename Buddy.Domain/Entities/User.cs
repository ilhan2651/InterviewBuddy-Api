using System.Collections.Generic;

namespace Buddy.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public ICollection<InterviewSession> InterviewSessions { get; set; } = new List<InterviewSession>();

        // 1-to-1 relationship for API Keys
        public UserApiKey? ApiKeys { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    }
}
