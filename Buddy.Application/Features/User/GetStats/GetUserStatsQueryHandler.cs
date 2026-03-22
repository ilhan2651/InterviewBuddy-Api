using Buddy.Application.Common.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.User.GetStats
{
    public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, GetUserStatsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetUserStatsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<GetUserStatsResponse> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? 0;
            var userSessions = (await _unitOfWork.InterviewSessions.GetCompletedSessionsByUserIdAsync(userId, cancellationToken)).ToList();

            if (!userSessions.Any())
            {
                return new GetUserStatsResponse
                {
                    TotalInterviews = 0,
                    TechnicalScore = 0,
                    CommunicationScore = 0,
                    ConfidenceScore = 0
                };
            }

            var allAnswers = userSessions
                .SelectMany(s => s.Questions)
                .Where(q => q.Answer != null)
                .Select(q => q.Answer!.Score ?? 0)
                .ToList();

            if (!allAnswers.Any())
            {
                return new GetUserStatsResponse
                {
                    TotalInterviews = userSessions.Count,
                    TechnicalScore = 0,
                    CommunicationScore = 0,
                    ConfidenceScore = 0
                };
            }

            var avgScore = (int)Math.Round(allAnswers.Average());
            var sessionsWithCommunication = userSessions.Where(s => s.CommunicationScore.HasValue).ToList();
            var sessionsWithConfidence = userSessions.Where(s => s.ConfidenceScore.HasValue).ToList();

            var communicationScore = sessionsWithCommunication.Any()
                ? (int)Math.Round(sessionsWithCommunication.Average(s => s.CommunicationScore ?? avgScore))
                : avgScore;

            var confidenceScore = sessionsWithConfidence.Any()
                ? (int)Math.Round(sessionsWithConfidence.Average(s => s.ConfidenceScore ?? avgScore))
                : avgScore;

            return new GetUserStatsResponse
            {
                TotalInterviews = userSessions.Count,
                TechnicalScore = avgScore,
                CommunicationScore = communicationScore,
                ConfidenceScore = confidenceScore
            };
        }
    }
}
