using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.User.GetRecentInterviews
{
    public class GetRecentInterviewsQueryHandler : IRequestHandler<GetRecentInterviewsQuery, List<RecentInterviewDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetRecentInterviewsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<RecentInterviewDto>> Handle(GetRecentInterviewsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? 0;
            var recentSessions = (await _unitOfWork.InterviewSessions.GetRecentCompletedSessionsByUserIdAsync(userId, 5, cancellationToken)).ToList();

            var result = recentSessions.Select(session =>
            {
                var answers = session.Questions
                    .Where(q => q.Answer != null)
                    .Select(q => q.Answer!.Score ?? 0)
                    .ToList();

                var avgScore = answers.Any() ? (int)answers.Average() : 0;

                return new RecentInterviewDto
                {
                    SessionId = session.Id,
                    PublicSessionId = session.SessionId,
                    Role = session.Role ?? "Genel Mülakat",
                    Score = avgScore,
                    Date = session.CreatedAt.ToString("dd MMMM yyyy HH:mm", new System.Globalization.CultureInfo("tr-TR"))
                };
            }).ToList();

            return result;
        }
    }
}
