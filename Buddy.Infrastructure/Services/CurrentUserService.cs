using Buddy.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Buddy.Infrastructure.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        public int? UserId
        {
            get
            {
                var rawId =
                    User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                return int.TryParse(rawId, out var id) ? id : null;
            }
        }

        public string? Email =>
            User?.FindFirst(ClaimTypes.Email)?.Value ??
            User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value;



    }
}
