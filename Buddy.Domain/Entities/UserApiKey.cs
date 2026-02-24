namespace Buddy.Domain.Entities
{
    public class UserApiKey : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string? SimliApiKey { get; set; }
        public string? ElevenLabsApiKey { get; set; }
    }
}
