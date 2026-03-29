using Buddy.Application.Features.Interview.StartInterview;
using System.IO;
using Buddy.Application.Common.Interfaces;
using Buddy.Application.Dtos.Interview;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Buddy.Application.Features.Interview.SubmitAnswer
{
    public class SubmitInterviewAnswerCommandHandler : IRequestHandler<SubmitInterviewAnswerCommand, SubmitInterviewAnswerResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInterviewLLMService _interviewLLMService;
        private readonly IGlobalCache _globalCache;
        private readonly ITextToSpeechService _ttsService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Microsoft.Extensions.Logging.ILogger<SubmitInterviewAnswerCommandHandler> _logger;

        public SubmitInterviewAnswerCommandHandler(IUnitOfWork unitOfWork, IInterviewLLMService interviewLLMService, IGlobalCache globalCache, ITextToSpeechService ttsService, IServiceScopeFactory scopeFactory, Microsoft.Extensions.Logging.ILogger<SubmitInterviewAnswerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _interviewLLMService = interviewLLMService;
            _globalCache = globalCache;
            _ttsService = ttsService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<SubmitInterviewAnswerResponse> Handle(SubmitInterviewAnswerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing submitted answer for SessionId: {SessionId}, QuestionId: {QuestionId}", request.SessionId, request.QuestionId);

            // 1. Get Session (Try Redis first, fallback to DB)
            var cacheKey = $"rsi:session:{request.SessionId}";

            var session = await _globalCache.GetAsync<InterviewSession>(cacheKey, cancellationToken);

            if (session == null)
            {
                _logger.LogInformation("Session {SessionId} not in cache, falling back to database.", request.SessionId);
                // Fallback to DB if not in Cache (e.g. expired or server restart)
                session = await _unitOfWork.InterviewSessions.GetBySessionIdAsync(request.SessionId, cancellationToken);

                // If found in DB, put it back in Cache
                if (session != null)
                {
                    _logger.LogInformation("Session {SessionId} restored from DB to cache.", request.SessionId);
                    await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);
                }
            }

            if (session == null)
            {
                _logger.LogError("Interview session {SessionId} not found in DB or Cache.", request.SessionId);
                throw new Exception("Interview session not found.");
            }

            var currentQuestion = session.Questions.FirstOrDefault(q => q.Id == request.QuestionId);
            if (currentQuestion == null)
            {
                _logger.LogError("Question {QuestionId} not found in Session {SessionId}.", request.QuestionId, request.SessionId);
                throw new Exception("Question not found.");
            }

            // 1.5 Handle Audio Input and Transcription
            string feedback = string.Empty;
            if (!string.IsNullOrEmpty(request.AudioPath) && string.IsNullOrEmpty(request.AnswerText))
            {
                _logger.LogInformation("Received audio submission. Proceeding to STT transcription. AudioPath: {AudioPath}", request.AudioPath);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", request.AudioPath);
                if (File.Exists(filePath))
                {
                    using var audioStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    try 
                    {
                        var transcription = await _ttsService.SpeechToTextAsync(audioStream, cancellationToken);
                        _logger.LogInformation("STT evaluation complete. Extracted text length: {TextLength}", transcription?.Length ?? 0);
                        
                        // Validation: Check if transcription is meaningful
                        if (string.IsNullOrWhiteSpace(transcription) || transcription.Length < 5)
                        {
                            _logger.LogWarning("STT failed or returned too short string. Returning [SES_ANLASILAMADI].");
                            // Soft Fail: Mark as unintelligible but continue
                            request.AnswerText = "[SES_ANLASILAMADI]";
                            feedback = "Ses kaydı net olmadığı veya çok kısa olduğu için bu soru değerlendirilemedi.";
                        }
                        else
                        {
                            request.AnswerText = transcription;
                        }
                    } 
                    catch (Exception ex)
                    {
                        // Log error
                        _logger.LogError(ex, "Failed to transcribe audio file using STT.");
                        request.AnswerText = "[SES_HATA]";
                        feedback = "Ses işlenirken teknik bir hata oluştu, değerlendirilemedi.";
                    }
                }
                else
                {
                     _logger.LogWarning("Audio file to transcribe not found on disk at {FilePath}", filePath);
                }
            }

                bool isFollowUpResponse = currentQuestion.ParentId.HasValue;
                bool shouldAskFollowUp = !isFollowUpResponse && (currentQuestion.Order == 2 || currentQuestion.Order == 6 || currentQuestion.Order == 8);
                
                // --- PERFORMANCE OPTIMIZATION: DETERMINISTIC FOLLOW-UPS & BACKGROUND EVAL ---
                
                feedback = "Cevabınız alındı. Uzman yapay zekalarımız değerlendirmeyi arka planda yapıyor.";

                // 2. Save Answer immediately
                var initialAnswer = new InterviewAnswer
                {
                    InterviewQuestionId = request.QuestionId,
                    UserAnswerText = request.AnswerText,
                    UserAudioPath = request.AudioPath,
                    AnsweredAt = DateTime.UtcNow,
                    AIAnalysis = "Değerlendiriliyor...", 
                    Score = 0
                };

                currentQuestion.Answer = initialAnswer;
                await _unitOfWork.InterviewAnswers.AddAsync(initialAnswer);
                
                // Save context so background task can find the answer record
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 3. Background Detailed Assessment (Asynchronous)
                // We fire-and-forget the scoring, feedback, vision, and audio tone analysis
                _logger.LogInformation("Enqueuing background evaluation for Session: {SessionId}, Question: {QuestionId}", request.SessionId, currentQuestion.Id);
                
                // Capture data for background closure (immutable copies)
                var snapshot = request.Base64Snapshot;
                var audioPath = request.AudioPath;
                var answerText = request.AnswerText;
                var sessionProfession = session.Profession;
                var sessionRole = session.Role;
                var sessionLevel = session.Level;
                var sessionDifficulty = session.Difficulty;
                var sessionLanguage = session.Language;
                var qId = currentQuestion.Id;
                var qText = currentQuestion.QuestionText;

                _ = Task.Run(async () => {
                    try 
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var bgUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var bgLlm = scope.ServiceProvider.GetRequiredService<IInterviewLLMService>();
                        var bgLogger = scope.ServiceProvider.GetRequiredService<ILogger<SubmitInterviewAnswerCommandHandler>>();

                        bgLogger.LogInformation("[BG] Starting background evaluation for QuestionId: {QuestionId}", qId);

                        // Only the textual answer is evaluated per-question.
                        var t_text = bgLlm.EvaluateInterviewAnswerAsync(qText, answerText, sessionProfession, sessionRole, sessionLevel, sessionDifficulty, sessionLanguage);
                        await t_text;

                        var resText = await t_text;
                        var bgFinalScore = resText.Score;
                        string idealAnswerSummary = string.Empty;

                        if (!string.IsNullOrWhiteSpace(answerText) &&
                            answerText != "[SES_ANLASILAMADI]" &&
                            answerText != "[SES_HATA]")
                        {
                            idealAnswerSummary = await bgLlm.GenerateIdealAnswerSummaryAsync(
                                qText,
                                answerText,
                                resText.Feedback,
                                sessionProfession,
                                sessionRole,
                                sessionLevel,
                                sessionDifficulty,
                                sessionLanguage,
                                CancellationToken.None);
                        }

                        // Persist to DB - use GetRepository for simplicity
                        var repo = bgUow.GetRepository<InterviewAnswer>();
                        var dbAnswer = await repo.GetQueryable()
                            .FirstOrDefaultAsync(x => x.InterviewQuestionId == qId, CancellationToken.None);

                        if (dbAnswer != null)
                        {
                            dbAnswer.AIAnalysis = resText.Feedback;
                            dbAnswer.Score = bgFinalScore;
                            dbAnswer.IdealAnswerSummary = idealAnswerSummary;
                            await bgUow.SaveChangesAsync(CancellationToken.None);
                            bgLogger.LogInformation("[BG] Background evaluation COMPLETED for {QuestionId}. Score: {Score}", qId, bgFinalScore);
                        }
                        else
                        {
                            bgLogger.LogWarning("[BG] Could not find InterviewAnswer in DB for QuestionId: {QuestionId}", qId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BG ERROR] Evaluation failed for {qId}: {ex.Message}");
                    }
                });

                if (shouldAskFollowUp)
                {
                    _logger.LogInformation("Deterministic follow-up triggered for Session: {SessionId}, Order: {Order}", request.SessionId, currentQuestion.Order);
                    
                    // Generate ONLY the question text using AI
                    var followUpInfo = await _interviewLLMService.DecideFollowUpAsync(
                        currentQuestion.QuestionText,
                        request.AnswerText,
                        session.Language,
                        cancellationToken);

                    var followUpText = followUpInfo.FollowUpQuestion ?? (session.Language.ToLower().Contains("turk") ? "Lütfen bu cevabınızı biraz daha detaylandırabilir misiniz?" : "Could you please elaborate on this answer?");
                    
                    string audioUrl = string.Empty;
                    try
                    {
                        var audioStream = await _ttsService.TextToSpeechAsync(followUpText, cancellationToken);
                        var fileName = $"followup_{currentQuestion.Id}_{Guid.NewGuid()}.mp3";
                        audioUrl = await _ttsService.SaveAudioAsync(audioStream, fileName, cancellationToken);
                    }
                    catch (Exception ex) { _logger.LogError(ex, "Follow-up TTS failed."); }

                    var followUpQuestion = new InterviewQuestion
                    {
                        InterviewSessionId = session.Id,
                        ParentId = currentQuestion.Id,
                        QuestionText = followUpText,
                        Type = currentQuestion.Type,
                        Order = currentQuestion.Order,
                        AudioUrl = audioUrl
                    };

                    await _unitOfWork.GetRepository<InterviewQuestion>().AddAsync(followUpQuestion);
                    session.Questions.Add(followUpQuestion);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    // Update Cache
                    await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);

                    return new SubmitInterviewAnswerResponse
                    {
                        IsCompleted = false,
                        Feedback = "Ek soru.",
                        NextQuestion = new InterviewQuestionDto
                        {
                            Id = followUpQuestion.Id,
                            Text = followUpText,
                            Order = followUpQuestion.Order,
                            DisplayNumber = $"{currentQuestion.Order}.1",
                            Type = followUpQuestion.Type,
                            AudioUrl = audioUrl,
                            ImageUrl = followUpQuestion.ImageUrl,
                            CodeSnippet = followUpQuestion.CodeSnippet
                        }
                    };
                }


                // Update Database (Persistence)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Update Redis (Reflect changes)
                await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);


                // 4. Get Next Question
                // Determine next sequential question ID logically. Since follow-ups have same Order but different ParentId logic, 
                // we want the NEXT base question.
                InterviewQuestion? nextQuestion = null;

                if (isFollowUpResponse)
                {
                    // If we just answered a follow-up, the next question is the base question AFTER our parent.
                    nextQuestion = session.Questions
                        .Where(q => q.ParentId == null && q.Order > currentQuestion.Order)
                        .OrderBy(q => q.Order)
                        .FirstOrDefault();
                }
                else
                {
                    // If we just answered a base question mapping and didn't branch to follow up above,
                    // next is the next base question.
                    nextQuestion = session.Questions
                        .Where(q => q.ParentId == null && q.Order > currentQuestion.Order)
                        .OrderBy(q => q.Order)
                        .FirstOrDefault();
                }

                if (nextQuestion != null)
                {
                    _logger.LogInformation("Routing user to next question {NextQuestionId} (Order: {Order})", nextQuestion.Id, nextQuestion.Order);
                    if (string.IsNullOrEmpty(nextQuestion.AudioUrl))
                    {
                        try
                        {
                            var audioStream = await _ttsService.TextToSpeechAsync(nextQuestion.QuestionText);
                            var fileName = $"q_{session.Id}_{nextQuestion.Id}_{Guid.NewGuid()}.mp3";
                            nextQuestion.AudioUrl = await _ttsService.SaveAudioAsync(audioStream, fileName);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("Next question TTS generated: {AudioUrl}", nextQuestion.AudioUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "TTS Error generating voice for the next question.");
                        }
                    }

                    // Calculate DisplayNumber
                    var baseQuestions = session.Questions.Where(q => q.ParentId == null).OrderBy(q => q.Id).ToList();
                    var baseIndex = baseQuestions.IndexOf(nextQuestion) + 1;

                    // Update Redis
                    await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);

                    return new SubmitInterviewAnswerResponse
                    {
                        IsCompleted = false,
                        Feedback = feedback,
                        NextQuestion = new InterviewQuestionDto
                        {
                            Id = nextQuestion.Id,
                            Text = nextQuestion.QuestionText,
                            Order = nextQuestion.Order,
                            DisplayNumber = baseIndex.ToString(),
                            Type = nextQuestion.Type,
                            AudioUrl = nextQuestion.AudioUrl,
                            ImageUrl = nextQuestion.ImageUrl,
                            CodeSnippet = nextQuestion.CodeSnippet
                        }
                    };
                }

                // 5. Complete Interview (No more questions)
                _logger.LogInformation("Interview session {SessionId} completed.", session.Id);
                session.CompletedAt = DateTime.UtcNow;

                // Validate context tracks session updates if we pulled from Redis and not context
                // Since we added 'answer' to context separately, we need to update session completion status
                var dbSession = await _unitOfWork.InterviewSessions.GetByIdAsync(session.Id);
                if (dbSession != null)
                {
                    dbSession.CompletedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Updated Interview Session Completion State in DB.");
                }

                // Update Redis
                await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);

                return new SubmitInterviewAnswerResponse
                {
                    IsCompleted = true,
                    Feedback = feedback,
                    CompletionMessage = "Interview process completed. Generating final report..."
                };
            }
        }
    } 

