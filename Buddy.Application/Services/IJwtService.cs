using Buddy.Domain.Entities;

namespace Buddy.Application.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
