using Buddy.Domain.Enums;
using System;

namespace Buddy.Application.Features.Chat.GetHistory
{
    public class ChatHistoryResponse
    {
        public int Id { get; set; }
        public MessageType Type { get; set; }
        public string TextContent { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
