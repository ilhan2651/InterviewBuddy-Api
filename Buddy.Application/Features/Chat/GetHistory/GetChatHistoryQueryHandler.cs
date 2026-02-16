using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Chat.GetHistory
{
    public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, List<ChatHistoryResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetChatHistoryQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ChatHistoryResponse>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
        {
            var messages = await _unitOfWork.Messages.GetMessagesBySessionIdAsync(request.SessionId, cancellationToken);

            return messages.Select(m => new ChatHistoryResponse
            {
                Id = m.Id,
                Type = m.Type,
                TextContent = m.TextContent,
                AudioUrl = m.AudioPath,
                CreatedAt = m.CreatedAt
            }).ToList();
        }
    }
}
