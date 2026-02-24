using Buddy.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Admin.GetSessions
{
    public class GetAdminSessionsQueryHandler : IRequestHandler<GetAdminSessionsQuery, List<AdminSessionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAdminSessionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AdminSessionDto>> Handle(GetAdminSessionsQuery request, CancellationToken cancellationToken)
        {
            var sessions = await _unitOfWork.InterviewSessions.GetQueryable()
                .Include(s => s.Questions)
                .ThenInclude(q => q.Answer)
                .Where(s => s.UserId == request.UserId)
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);

            return sessions.Select(s => new AdminSessionDto
            {
                SessionId = s.Id,
                Role = s.Role,
                Level = s.Level.ToString(),
                Language = s.Language,
                CreatedAt = s.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                CompletedAt = s.CompletedAt?.ToString("dd/MM/yyyy HH:mm"),
                TotalQuestions = s.Questions.Count,
                AnsweredQuestions = s.Questions.Count(q => q.Answer != null),
                OverallScore = s.Questions.Where(q => q.Answer != null).Any() 
                    ? (int)s.Questions.Where(q => q.Answer != null).Average(q => q.Answer!.Score ?? 0) 
                    : 0
            }).ToList();
        }
    }
}
