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
        private readonly ILLMService _openAIService;
        private readonly IGlobalCache _globalCache;

        public CompleteInterviewCommandHandler(IUnitOfWork unitOfWork, ILLMService openAIService, IGlobalCache globalCache)
        {
            _unitOfWork = unitOfWork;
            _openAIService = openAIService;
            _globalCache = globalCache;
        }

        public async Task<CompleteInterviewResponse> Handle(CompleteInterviewCommand request, CancellationToken cancellationToken)
        {
            var cacheKey = $"rsi:session:{request.InterviewSessionId}";
            var session = await _globalCache.GetAsync<InterviewSession>(cacheKey, cancellationToken);
            
            if (session == null)
            {
                session = await _unitOfWork.InterviewSessions.GetWithDetailsAsync(request.InterviewSessionId, cancellationToken);
            }

            if (session == null) throw new Exception("Interview session not found.");

            // Generate Final Feedback
            var finalReport = await _openAIService.GenerateFinalFeedbackAsync(session.Role, session.Level, session.Questions.ToList());

            // Save to DB
            var dbSession = await _unitOfWork.InterviewSessions.GetByIdAsync(session.Id);
            if (dbSession != null)
            {
                dbSession.FinalFeedback = finalReport;
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
