using System;
using Buddy.Domain.Enums;

namespace Buddy.Domain.Entities
{
    public class Message : BaseEntity
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public MessageType Type { get; set; }
        public string TextContent { get; set; } = string.Empty;
        public string? AudioPath { get; set; }
    }
}
