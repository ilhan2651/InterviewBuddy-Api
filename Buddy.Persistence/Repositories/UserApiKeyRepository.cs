using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;

namespace Buddy.Persistence.Repositories
{
    public class UserApiKeyRepository : GenericRepository<UserApiKey>, IUserApiKeyRepository
    {
        public UserApiKeyRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
