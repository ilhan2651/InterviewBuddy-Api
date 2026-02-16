using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Persistence.Repositories
{
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConversationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Conversation?> GetWithMessagesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Conversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .FirstOrDefaultAsync(c => c.SessionId == sessionId, cancellationToken);
        }

        public async Task<Conversation?> GetBySessionIdWithMessagesAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId, cancellationToken);
        }
    }
}
