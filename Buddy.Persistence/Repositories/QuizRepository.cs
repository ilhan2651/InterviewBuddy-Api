using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Persistence.Repositories
{
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        }
    }
}
