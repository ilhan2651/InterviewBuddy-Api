using MediatR;
using System.Collections.Generic;

namespace Buddy.Application.Features.Admin.GetUsers
{
    public class GetAdminUsersQuery : IRequest<List<AdminUserDto>>
    {
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalInterviews { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
