using Buddy.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Common.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<User?> GetWithInterviewsAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);


    }
}
