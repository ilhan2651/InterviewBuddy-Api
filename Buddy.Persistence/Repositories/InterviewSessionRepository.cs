using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Persistence.Repositories
{
    public class InterviewSessionRepository : GenericRepository<InterviewSession>, IInterviewSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewSessionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<InterviewSession?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.InterviewSessions
                .Include(s => s.Questions)
                .ThenInclude(q => q.Answer)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<InterviewSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.InterviewSessions
                .Include(s => s.Questions)
                .ThenInclude(q => q.Answer)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        }

        public async Task<IEnumerable<InterviewSession>> GetCompletedSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.InterviewSessions
                .Include(s => s.Questions)
                .ThenInclude(q => q.Answer)
                .Where(s => s.UserId == userId && s.CompletedAt.HasValue)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InterviewSession>> GetRecentCompletedSessionsByUserIdAsync(int userId, int count, CancellationToken cancellationToken = default)
        {
            return await _context.InterviewSessions
                .Include(s => s.Questions)
                .ThenInclude(q => q.Answer)
                .Where(s => s.UserId == userId && s.CompletedAt.HasValue)
                .OrderByDescending(s => s.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
