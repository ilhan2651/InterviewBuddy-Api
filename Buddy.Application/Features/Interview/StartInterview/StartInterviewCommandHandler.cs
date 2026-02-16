using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Interview.StartInterview
{
    public class StartInterviewCommandHandler : IRequestHandler<StartInterviewCommand, StartInterviewResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILLMService _openAIService;
        private readonly IGlobalCache _globalCache;
        private readonly ICurrentUserService _currentUserService;

        public StartInterviewCommandHandler(IUnitOfWork unitOfWork, ILLMService openAIService, IGlobalCache globalCache, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _openAIService = openAIService;
            _globalCache = globalCache;
            _currentUserService = currentUserService;
        }

        public async Task<StartInterviewResponse> Handle(StartInterviewCommand request, CancellationToken cancellationToken)
        {
            // 1. Create Interview Session
            var session = new InterviewSession
            {
                UserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("Authenticated user ID not found."),
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                Role = request.JobTitle,
                Level = request.Level,
                StartedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<InterviewSession>().AddAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2. Generate Questions
            var questions = new List<InterviewQuestion>();
            int order = 1;

            // 2.1 Intro
            questions.Add(new InterviewQuestion
            {
                InterviewSessionId = session.Id,
                QuestionText = $"Hello! Welcome to your interview for the {request.Level} {request.JobTitle} position. Could you briefly introduce yourself and tell me about your background?",
                Type = InterviewQuestionType.Intro,
                Order = order++
            });

            // 2.2 Behavioral (3 Questions)
            var behavioralQuestions = await _openAIService.GenerateInterviewQuestionsAsync(
                request.JobTitle, request.Level, InterviewQuestionType.Behavioral, 3);
            
            foreach (var qText in behavioralQuestions)
            {
                questions.Add(new InterviewQuestion { InterviewSessionId = session.Id, QuestionText = qText, Type = InterviewQuestionType.Behavioral, Order = order++ });
            }

            // 2.3 Technical (5 Questions)
            var technicalQuestions = await _openAIService.GenerateInterviewQuestionsAsync(
                request.JobTitle, request.Level, InterviewQuestionType.Technical, 5);

            foreach (var qText in technicalQuestions)
            {
                questions.Add(new InterviewQuestion { InterviewSessionId = session.Id, QuestionText = qText, Type = InterviewQuestionType.Technical, Order = order++ });
            }

            // 2.4 Closing
            questions.Add(new InterviewQuestion
            {
                InterviewSessionId = session.Id,
                QuestionText = "That concludes our technical section. Do you have any final questions for me before we finish?",
                Type = InterviewQuestionType.Closing,
                Order = order++
            });

            var questionRepository = _unitOfWork.GetRepository<InterviewQuestion>();
            foreach (var question in questions)
            {
                await questionRepository.AddAsync(question);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2.5 Cache Session in Redis
            // We reload the session with questions to ensure we have the full graph
            // Or we can construct it manually to save a DB trip, but reloading is safer for consistency.
            // Actually, we have the objects in memory, let's link them and cache.
            session.Questions = questions;
            
            // Cache Key: rsi:session:{id}
            await _globalCache.SetAsync($"rsi:session:{session.Id}", session, TimeSpan.FromHours(2), false, cancellationToken);

            // 3. Return Response
            var firstQuestion = questions.First();

            return new StartInterviewResponse
            {
                InterviewSessionId = session.Id,
                SessionId = session.SessionId,
                TotalQuestions = questions.Count,
                FirstQuestion = new InterviewQuestionDto
                {
                    Id = firstQuestion.Id,
                    Text = firstQuestion.QuestionText,
                    Order = firstQuestion.Order,
                    Type = firstQuestion.Type
                }
            };
        }
    }
}
