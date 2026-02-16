namespace Buddy.Application.Features.Chat.SendTextMessage
{
    public class SendTextMessageResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string UserMessage { get; set; } = string.Empty;
        public string AIResponse { get; set; } = string.Empty;
        public string AIAudioUrl { get; set; } = string.Empty;
    }
}
