using MediatR;

namespace Buddy.Application.Features.Chat.SendTextMessage
{
    public class SendTextMessageCommand : IRequest<SendTextMessageResponse>
    {
        public string? AnonymousId { get; set; }
        public string? SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
