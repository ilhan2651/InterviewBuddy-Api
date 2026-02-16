using Buddy.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Common.Interfaces
{
    public interface IInterviewSessionRepository : IGenericRepository<InterviewSession>
    {
        Task<InterviewSession?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<InterviewSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InterviewSession>> GetCompletedSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InterviewSession>> GetRecentCompletedSessionsByUserIdAsync(int userId, int count, CancellationToken cancellationToken = default);
    }
}
