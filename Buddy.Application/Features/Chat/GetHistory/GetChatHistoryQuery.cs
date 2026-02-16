using MediatR;
using System.Collections.Generic;

namespace Buddy.Application.Features.Chat.GetHistory
{
    public class GetChatHistoryQuery : IRequest<List<ChatHistoryResponse>>
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
