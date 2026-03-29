using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u=>u.EmailVerificationToken == token, cancellationToken);
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
        }

        public async Task<User?> GetWithInterviewsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.InterviewSessions)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
    }
}
