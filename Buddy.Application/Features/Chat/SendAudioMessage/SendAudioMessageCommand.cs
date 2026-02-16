using MediatR;
using System.IO;
using Buddy.Application.Features.Chat.SendTextMessage;

namespace Buddy.Application.Features.Chat.SendAudioMessage
{
    public class SendAudioMessageCommand : IRequest<SendTextMessageResponse>
    {
        public string? AnonymousId { get; set; }
        public string? SessionId { get; set; }
        public Stream AudioStream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
    }
}
