using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Interview.CompleteInterview
{
    public class CompleteInterviewCommandHandler : IRequestHandler<CompleteInterviewCommand, CompleteInterviewResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInterviewLLMService _interviewLLMService;
        private readonly IGlobalCache _globalCache;

        public CompleteInterviewCommandHandler(IUnitOfWork unitOfWork, IInterviewLLMService interviewLLMService, IGlobalCache globalCache)
        {
            _unitOfWork = unitOfWork;
            _interviewLLMService = interviewLLMService;
            _globalCache = globalCache;
        }

        public async Task<CompleteInterviewResponse> Handle(CompleteInterviewCommand request, CancellationToken cancellationToken)
        {
            var cacheKey = $"rsi:session:{request.SessionId}";
            var session = await _globalCache.GetAsync<InterviewSession>(cacheKey, cancellationToken);
            
            if (session == null)
            {
                session = await _unitOfWork.InterviewSessions.GetBySessionIdAsync(request.SessionId, cancellationToken);
            }

            if (session == null) throw new Exception("Interview session not found.");

            // Generate Final Feedback
            var finalReport = await _interviewLLMService.GenerateFinalFeedbackAsync(session.Profession ?? "Genel Mülakat", session.Role, session.Level, session.Difficulty, session.Questions.ToList(), session.Language, cancellationToken);

            var sessionAssessment = await _interviewLLMService.GenerateSessionAssessmentAsync
                (
                session.Profession ?? "Genel Mülakat",
                session.Role,
                session.Level,
                session.Difficulty,
                session.Language,
                session.Questions.ToList(),
                cancellationToken
                );

            // Save to DB
            var dbSession = await _unitOfWork.InterviewSessions.GetByIdAsync(session.Id);
            if (dbSession != null)
            {
                dbSession.FinalFeedback = finalReport;
                dbSession.CommunicationScore = sessionAssessment.CommunicationScore;
                dbSession.CommunicationFeedback = sessionAssessment.CommunicationFeedback;
                dbSession.ConfidenceScore = sessionAssessment.ConfidenceScore;
                dbSession.ConfidenceFeedback = sessionAssessment.ConfidenceFeedback;

                dbSession.CompletedAt = DateTime.UtcNow; // Ensure it's marked as complete
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Clear Cache (Cleanup)
            await _globalCache.RemoveAsync(cacheKey, cancellationToken);

            return new CompleteInterviewResponse
            {
                TotalScore = 0, // Placeholder
                FinalFeedbackReport = finalReport
            };
        }
    }
}
