using Buddy.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Admin.GetUsers
{
    public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, List<AdminUserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAdminUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Users.GetQueryable()
                .AsNoTracking()
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    TotalInterviews = u.InterviewSessions.Count,
                    CreatedAt = u.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .OrderByDescending(u => u.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
