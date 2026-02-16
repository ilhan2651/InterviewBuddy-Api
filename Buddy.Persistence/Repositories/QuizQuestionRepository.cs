using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Persistence.Repositories
{
    public class QuizQuestionRepository : GenericRepository<QuizQuestion>, IQuizQuestionRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizQuestionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<QuizQuestion?> GetWithQuizAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.QuizQuestions
                .Include(qq => qq.Quiz)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(qq => qq.Id == id, cancellationToken);
        }
    }
}
