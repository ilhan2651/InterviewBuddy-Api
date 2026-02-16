using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Interview.GetCurrentQuestion
{
    public class GetCurrentQuestionQueryHandler : IRequestHandler<GetCurrentQuestionQuery, GetCurrentQuestionResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCurrentQuestionQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetCurrentQuestionResponse> Handle(GetCurrentQuestionQuery request, CancellationToken cancellationToken)
        {
            var session = await _unitOfWork.InterviewSessions.GetWithDetailsAsync(request.SessionId, cancellationToken);

            if (session == null)
            {
                throw new Exception($"Interview session {request.SessionId} not found");
            }

            // Find first unanswered question or last question if all answered
            var currentQuestion = session.Questions
                .OrderBy(q => q.Id)
                .FirstOrDefault(q => q.Answer == null) 
                ?? session.Questions.OrderByDescending(q => q.Id).FirstOrDefault();

            if (currentQuestion == null)
            {
                throw new Exception("No questions found for this session");
            }

            var questionNumber = session.Questions.OrderBy(q => q.Id).ToList().IndexOf(currentQuestion) + 1;

            return new GetCurrentQuestionResponse
            {
                QuestionText = currentQuestion.QuestionText,
                QuestionNumber = questionNumber,
                AudioUrl = currentQuestion.AudioUrl ?? string.Empty
            };
        }
    }
}
