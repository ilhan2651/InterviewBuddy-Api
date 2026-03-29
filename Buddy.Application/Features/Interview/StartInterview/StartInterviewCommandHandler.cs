using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Buddy.Application.Features.Interview.StartInterview
{
    public class StartInterviewCommandHandler : IRequestHandler<StartInterviewCommand, StartInterviewResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInterviewLLMService _interviewLLMService;
        private readonly IGlobalCache _globalCache;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITextToSpeechService _ttsService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Microsoft.Extensions.Logging.ILogger<StartInterviewCommandHandler> _logger;

        public StartInterviewCommandHandler(IUnitOfWork unitOfWork, IInterviewLLMService interviewLLMService, IGlobalCache globalCache, ICurrentUserService currentUserService, ITextToSpeechService ttsService, IServiceScopeFactory scopeFactory, Microsoft.Extensions.Logging.ILogger<StartInterviewCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _interviewLLMService = interviewLLMService;
            _globalCache = globalCache;
            _currentUserService = currentUserService;
            _ttsService = ttsService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<StartInterviewResponse> Handle(StartInterviewCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling StartInterviewCommand for User ID: {UserId}, Session: {SessionId}", _currentUserService.UserId, request.SessionId);

            // 1. Create Interview Session
            var session = new InterviewSession
            {
                UserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("Authenticated user ID not found."),
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                Profession = request.Profession,
                Role = request.JobTitle,
                Level = request.Level,
                Difficulty = request.Difficulty,
                Language = request.Language,
                StartedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<InterviewSession>().AddAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("InterviewSession created successfully. ID: {InternalSessionId}, Public ID: {PublicSessionId}", session.Id, session.SessionId);

            // 1.5 Fetch previously asked questions for this role/level/language
            var pastSessions = await _unitOfWork.InterviewSessions.GetQueryable()
                .Include(s => s.Questions)
                .Where(s => s.UserId == session.UserId && 
                            s.Profession == request.Profession &&
                            s.Role == request.JobTitle && 
                            s.Level == request.Level && 
                            s.Difficulty == request.Difficulty &&
                            s.Language == request.Language &&
                            s.Id != session.Id) // exclude current session
                .ToListAsync(cancellationToken);

            var previouslyAskedQuestions = pastSessions
                .SelectMany(s => s.Questions)
                .Where(q => q.Type == InterviewQuestionType.Behavioral || q.Type == InterviewQuestionType.Technical)
                .Select(q => q.QuestionText)
                .Distinct()
                .ToList();

            if (previouslyAskedQuestions.Any())
            {
                _logger.LogInformation("Found {Count} previously asked questions to exclude for user {UserId}.", previouslyAskedQuestions.Count, session.UserId);
            }

            // 2. Generate Questions
            var questions = new List<InterviewQuestion>();
            int order = 1;

            // 2.1 Intro
            var isEnglish = request.Language != null && (request.Language.Equals("English", StringComparison.OrdinalIgnoreCase) || request.Language.Equals("İngilizce", StringComparison.OrdinalIgnoreCase));
            var introText = isEnglish ? "Welcome! We can start our interview. First, could you briefly tell me about yourself?" : "Hoş geldiniz! Mülakatımıza başlayabiliriz. Öncelikle kendinizden kısaca bahsedebilir misiniz?";
            
            questions.Add(new InterviewQuestion { InterviewSessionId = session.Id, QuestionText = introText, Type = InterviewQuestionType.Intro, Order = order++ });

            // 2.2 Behavioral (2 Questions)
            var behavioralResults = await _interviewLLMService.GenerateInterviewQuestionsAsync(
                request.Profession, request.JobTitle, request.Level, request.Difficulty, InterviewQuestionType.Behavioral, 2, request.Language, previouslyAskedQuestions, cancellationToken);
            
            foreach (var res in behavioralResults)
            {
                questions.Add(new InterviewQuestion { 
                    InterviewSessionId = session.Id, 
                    QuestionText = res.QuestionText, 
                    CodeSnippet = res.CodeSnippet,
                    Type = InterviewQuestionType.Behavioral, 
                    Order = order++ 
                });
                previouslyAskedQuestions.Add(res.QuestionText);
            }

            // 2.3 Technical (5 Questions)
            var technicalResults = await _interviewLLMService.GenerateInterviewQuestionsAsync(
                request.Profession, request.JobTitle, request.Level, request.Difficulty, InterviewQuestionType.Technical, 5, request.Language, previouslyAskedQuestions, cancellationToken);

            foreach (var res in technicalResults)
            {
                questions.Add(new InterviewQuestion { 
                    InterviewSessionId = session.Id, 
                    QuestionText = res.QuestionText, 
                    CodeSnippet = res.CodeSnippet,
                    Type = InterviewQuestionType.Technical, 
                    Order = order++ 
                });
                previouslyAskedQuestions.Add(res.QuestionText);
            }

            _logger.LogInformation("Persisting {Count} generated questions to DB.", questions.Count);

            await _unitOfWork.InterviewQuestions.AddRangeAsync(questions);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Pre-generate Text-to-Speech (TTS) for the first question synchronously to avoid delay
            var firstQuestion = questions.First();
            try
            {
                var audioStream = await _ttsService.TextToSpeechAsync(firstQuestion.QuestionText, session.Language, cancellationToken);
                var fileName = $"q_{session.Id}_{firstQuestion.Id}_{Guid.NewGuid()}.mp3";
                var audioUrl = await _ttsService.SaveAudioAsync(audioStream, fileName, cancellationToken);
                firstQuestion.AudioUrl = audioUrl;
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Intro TTS pre-generated: {AudioUrl}", audioUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TTS Error generating intro voice synchronously.");
            }

            // 4. Fire and forget TTS for remaining questions in the background
            var remainingQuestions = questions.Skip(1).ToList();
            if (remainingQuestions.Any())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var backgroundUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var backgroundTts = scope.ServiceProvider.GetRequiredService<ITextToSpeechService>();
                        var backgroundLogger = scope.ServiceProvider.GetRequiredService<ILogger<StartInterviewCommandHandler>>();

                        foreach (var q in remainingQuestions)
                        {
                            try
                            {
                                var stream = await backgroundTts.TextToSpeechAsync(q.QuestionText, session.Language, CancellationToken.None);
                                var name = $"q_{session.Id}_{q.Id}_{Guid.NewGuid()}.mp3";
                                var url = await backgroundTts.SaveAudioAsync(stream, name);

                                // Update DB record
                                var dbQuestion = await backgroundUow.GetRepository<InterviewQuestion>().GetByIdAsync(q.Id);
                                if (dbQuestion != null)
                                {
                                    dbQuestion.AudioUrl = url;
                                    await backgroundUow.SaveChangesAsync(CancellationToken.None);
                                }
                            }
                            catch (Exception ex)
                            {
                                backgroundLogger.LogError(ex, "Background TTS generation failed for Question ID {QuestionId}", q.Id);
                            }
                        }
                        backgroundLogger.LogInformation("Background TTS generation completed for Session {InternalSessionId}.", session.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Critical background TTS failure: {ex.Message}");
                    }
                });
            }

            // Cache Key: rsi:session:{id}
            _logger.LogInformation("Caching Interview Session {InternalSessionId}.", session.Id);
            await _globalCache.SetAsync($"rsi:session:{session.Id}", session, TimeSpan.FromHours(2), false, cancellationToken);

            // 5. Return Response
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
                    DisplayNumber = "1",
                    Type = firstQuestion.Type,
                    AudioUrl = firstQuestion.AudioUrl
                }
            };
        }
    }
}
