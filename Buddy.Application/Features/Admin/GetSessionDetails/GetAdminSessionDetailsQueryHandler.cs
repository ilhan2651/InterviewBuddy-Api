using Buddy.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Admin.GetSessionDetails
{
    public class GetAdminSessionDetailsQueryHandler : IRequestHandler<GetAdminSessionDetailsQuery, AdminSessionDetailsDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAdminSessionDetailsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminSessionDetailsDto> Handle(GetAdminSessionDetailsQuery request, CancellationToken cancellationToken)
        {
            var session = await _unitOfWork.InterviewSessions.GetQueryable()
                .Include(s => s.Questions)
                .ThenInclude(q => q.Answer)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
            {
                throw new Exception("Session not found");
            }

            return new AdminSessionDetailsDto
            {
                SessionId = session.Id,
                Role = session.Role ?? "Genel Mülakat",
                Level = session.Level.ToString(),
                Language = session.Language,
                CreatedAt = session.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                Questions = session.Questions.OrderBy(q => q.Order).Select(q => new AdminQuestionDto
                {
                    QuestionId = q.Id,
                    Order = q.Order,
                    QuestionText = q.QuestionText,
                    Answer = q.Answer == null ? null : new AdminAnswerDto
                    {
                        AnswerId = q.Answer.Id,
                        UserAnswerText = q.Answer.UserAnswerText ?? "",
                        AIAnalysis = q.Answer.AIAnalysis ?? "",
                        Score = q.Answer.Score ?? 0,
                        VideoScore = q.Answer.VideoScore,
                        VideoFeedback = q.Answer.VideoFeedback,
                        AudioScore = q.Answer.AudioScore,
                        AudioFeedback = q.Answer.AudioFeedback,
                        AnsweredAt = q.Answer.AnsweredAt.ToString("dd/MM/yyyy HH:mm")
                    }
                }).ToList()
            };
        }
    }
}
