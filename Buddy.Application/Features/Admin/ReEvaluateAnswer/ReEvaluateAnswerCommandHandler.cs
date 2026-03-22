using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Admin.ReEvaluateAnswer
{
    public class ReEvaluateAnswerCommandHandler : IRequestHandler<ReEvaluateAnswerCommand, ReEvaluateAnswerResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInterviewLLMService _interviewLLMService;

        public ReEvaluateAnswerCommandHandler(IUnitOfWork unitOfWork, IInterviewLLMService interviewLLMService)
        {
            _unitOfWork = unitOfWork;
            _interviewLLMService = interviewLLMService;
        }

        public async Task<ReEvaluateAnswerResponse> Handle(ReEvaluateAnswerCommand request, CancellationToken cancellationToken)
        {
            var answer = await _unitOfWork.InterviewAnswers.GetQueryable()
                .Include(a => a.InterviewQuestion)
                .ThenInclude(q => q.InterviewSession)
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer == null)
                throw new Exception("Answer not found");

            // Allow the admin to edit the text for testing evaluating different scenarios on the same question
            var textToEvaluate = !string.IsNullOrWhiteSpace(request.UpdatedAnswerText) 
                ? request.UpdatedAnswerText 
                : answer.UserAnswerText ?? "";

            var session = answer.InterviewQuestion.InterviewSession;

            var assessment = await _interviewLLMService.EvaluateInterviewAnswerAsync(
                answer.InterviewQuestion.QuestionText,
                textToEvaluate,
                session.Profession ?? "Genel Mülakat",
                session.Role ?? "Genel Mülakat",
                session.Level,
                session.Difficulty,
                session.Language);

            // Update DB record
            answer.UserAnswerText = textToEvaluate; // Save back the new edited text if any
            answer.AIAnalysis = assessment.Feedback;
            answer.Score = assessment.Score;
            // Note: We ignore "RequiresFollowUp" for the admin re-eval tool, as we just want the score/feedback

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ReEvaluateAnswerResponse
            {
                AnswerId = answer.Id,
                NewFeedback = answer.AIAnalysis,
                NewScore = answer.Score ?? 0
            };
        }
    }
}
