using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;

namespace Buddy.Persistence.Repositories
{
    public class InterviewAnswerRepository : GenericRepository<InterviewAnswer>, IInterviewAnswerRepository
    {
        public InterviewAnswerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
