using Buddy.Domain.Entities;

namespace Buddy.Application.Common.Interfaces
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<Message?> GetLastUserMessageInSessionAsync(string sessionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Message>> GetMessagesBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
