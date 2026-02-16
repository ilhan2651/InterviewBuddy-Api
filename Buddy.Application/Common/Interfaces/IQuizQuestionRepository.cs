using Buddy.Domain.Entities;

namespace Buddy.Application.Common.Interfaces
{
    public interface IQuizQuestionRepository : IGenericRepository<QuizQuestion>
    {
        Task<QuizQuestion?> GetWithQuizAsync(int id, System.Threading.CancellationToken cancellationToken = default);
    }
}
