using Buddy.Application.Common.Interfaces;
using Buddy.Application.Dtos.Quiz;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Quiz.StartQuiz
{
    public class StartQuizCommandHandler : IRequestHandler<StartQuizCommand, StartQuizResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILLMService _openAIService;
        private readonly ICurrentUserService _currentUserService;

        public StartQuizCommandHandler(IUnitOfWork unitOfWork, ILLMService openAIService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _openAIService = openAIService;
            _currentUserService = currentUserService;
        }

        public async Task<StartQuizResponse> Handle(StartQuizCommand request, CancellationToken cancellationToken)
        {
            // 1. Find or create conversation
            var conversation = await _unitOfWork.Conversations.GetBySessionIdAsync(request.SessionId, cancellationToken);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    UserId = _currentUserService.UserId,
                    AnonymousId = request.AnonymousId,
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    StartedAt = DateTime.UtcNow
                };
                await _unitOfWork.Conversations.AddAsync(conversation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 2. Create Quiz entity
            var quiz = new Buddy.Domain.Entities.Quiz
            {
                ConversationId = conversation.Id,
                Topic = request.Topic,
                Difficulty = request.Difficulty,
                QuestionCount = request.QuestionCount,
                QuestionsGeneratedAt = DateTime.UtcNow
            };
            await _unitOfWork.Quizzes.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Generate Questions
            var generatedQuestions = await _openAIService.GenerateQuizQuestionsAsync(
                request.Topic, 
                request.Difficulty, 
                request.QuestionCount);

            // 4. Save Questions
            foreach (var qDto in generatedQuestions)
            {
                var question = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    QuestionNumber = qDto.Number,
                    QuestionText = qDto.Question,
                    ExpectedKeywords = System.Text.Json.JsonSerializer.Serialize(qDto.ExpectedKeywords),
                    GeneratedAt = DateTime.UtcNow
                };
                await _unitOfWork.QuizQuestions.AddAsync(question);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Build Response (Return the first question)
            var firstQuestion = generatedQuestions.FirstOrDefault(q => q.Number == 1);

            return new StartQuizResponse
            {
                QuizId = quiz.Id,
                SessionId = conversation.SessionId,
                FirstQuestion = firstQuestion,
                TotalQuestions = generatedQuestions.Count
            };
        }
    }
}
