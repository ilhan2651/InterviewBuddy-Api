using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using Buddy.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Persistence.Repositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Message?> GetLastUserMessageInSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .Include(m => m.Conversation)
                .Where(m => m.Conversation.SessionId == sessionId && m.Type == MessageType.User)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Message>> GetMessagesBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .Include(m => m.Conversation)
                .Where(m => m.Conversation.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
