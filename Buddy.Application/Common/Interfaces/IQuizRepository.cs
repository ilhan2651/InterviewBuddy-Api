using Buddy.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Common.Interfaces
{
    public interface IQuizRepository : IGenericRepository<Quiz>
    {
        Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken cancellationToken = default);
    }
}
