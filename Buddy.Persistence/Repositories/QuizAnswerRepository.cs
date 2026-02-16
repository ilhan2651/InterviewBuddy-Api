using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;

namespace Buddy.Persistence.Repositories
{
    public class QuizAnswerRepository : GenericRepository<QuizAnswer>, IQuizAnswerRepository
    {
        public QuizAnswerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
