using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;

namespace Buddy.Persistence.Repositories
{
    public class InterviewQuestionRepository : GenericRepository<InterviewQuestion>, IInterviewQuestionRepository
    {
        public InterviewQuestionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
