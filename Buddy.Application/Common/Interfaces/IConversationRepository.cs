using Buddy.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Common.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<Conversation?> GetWithMessagesAsync(int id, CancellationToken cancellationToken = default);
        Task<Conversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
        Task<Conversation?> GetBySessionIdWithMessagesAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
