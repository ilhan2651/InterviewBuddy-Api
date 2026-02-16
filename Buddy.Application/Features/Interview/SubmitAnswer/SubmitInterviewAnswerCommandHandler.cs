using Buddy.Application.Features.Interview.StartInterview;
using System.IO;
using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Interview.SubmitAnswer
{
    public class SubmitInterviewAnswerCommandHandler : IRequestHandler<SubmitInterviewAnswerCommand, SubmitInterviewAnswerResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILLMService _openAIService;
        private readonly IGlobalCache _globalCache;

        public SubmitInterviewAnswerCommandHandler(IUnitOfWork unitOfWork, ILLMService openAIService, IGlobalCache globalCache)
        {
            _unitOfWork = unitOfWork;
            _openAIService = openAIService;
            _globalCache = globalCache;
        }

        public async Task<SubmitInterviewAnswerResponse> Handle(SubmitInterviewAnswerCommand request, CancellationToken cancellationToken)
        {
            // 1. Get Session (Try Redis first, fallback to DB)
            var cacheKey = $"rsi:session:{request.InterviewSessionId}";

            var session = await _globalCache.GetAsync<InterviewSession>(cacheKey, cancellationToken);

            if (session == null)
            {
                // Fallback to DB if not in Cache (e.g. expired or server restart)
                session = await _unitOfWork.InterviewSessions.GetWithDetailsAsync(request.InterviewSessionId, cancellationToken);

                // If found in DB, put it back in Cache
                if (session != null)
                {
                    await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);
                }
            }

            if (session == null) throw new Exception("Interview session not found.");

            var currentQuestion = session.Questions.FirstOrDefault(q => q.Id == request.QuestionId);
            if (currentQuestion == null) throw new Exception("Question not found.");

            // 1.5 Handle Audio Input and Transcription
            string feedback = string.Empty;
            if (!string.IsNullOrEmpty(request.AudioPath) && string.IsNullOrEmpty(request.AnswerText))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", request.AudioPath);
                if (File.Exists(filePath))
                {
                    using var audioStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    try 
                    {
                        var transcription = await _openAIService.TranscribeAudioAsync(audioStream);
                        
                        // Validation: Check if transcription is meaningful
                        if (string.IsNullOrWhiteSpace(transcription) || transcription.Length < 5)
                        {
                            // Soft Fail: Mark as unintelligible but continue
                            request.AnswerText = "[SES_ANLASILAMADI]";
                            feedback = "Ses kaydı net olmadığı veya çok kısa olduğu için bu soru değerlendirilemedi.";
                        }
                        else
                        {
                            request.AnswerText = transcription;
                        }
                    } 
                    catch (Exception)
                    {
                        // Log error
                        request.AnswerText = "[SES_HATA]";
                        feedback = "Ses işlenirken teknik bir hata oluştu, değerlendirilemedi.";
                    }
                }
            }

                // 2. Evaluate Answer & Handle Follow-up logic
                AssessmentResult assessment = new AssessmentResult();

                // Check if we are answering a follow-up (i.e., answer already exists)
                var existingAnswer = currentQuestion.Answer;
                bool isFollowUpResponse = existingAnswer != null;

                // Final validation for empty answer
                if (string.IsNullOrWhiteSpace(request.AnswerText))
                {
                    return new SubmitInterviewAnswerResponse
                    {
                        IsCompleted = false,
                        RetryRequired = true,
                        ErrorMessage = "Lütfen bir cevap verin (Ses veya Yazı).",
                        Feedback = "Cevap yok."
                    };
                }

                if (!string.IsNullOrWhiteSpace(request.AnswerText))
                {
                    // If this is a follow-up, we might want to pass context, but for now let's evaluate the specific response
                    assessment = await _openAIService.EvaluateInterviewAnswerAsync(
                        isFollowUpResponse ? $"Follow-up: {currentQuestion.QuestionText}" : currentQuestion.QuestionText,
                        request.AnswerText,
                        session.Role,
                        session.Level);
                }

                feedback = assessment.Feedback;
                
                if (existingAnswer != null)
                {
                    // Append to existing answer
                    existingAnswer.UserAnswerText += $"\n\n[Follow-up Answer]: {request.AnswerText}";
                    existingAnswer.AIAnalysis += $"\n\n[Follow-up Feedback]: {assessment.Feedback}";
                    existingAnswer.AnsweredAt = DateTime.UtcNow;
                    // We don't allow double follow-ups for now to keep it simple
                    // Proceed to next question
                }
                else
                {
                    // First time answering this question
                    var answer = new InterviewAnswer
                    {
                        InterviewQuestionId = request.QuestionId,
                        UserAnswerText = request.AnswerText,
                        UserAudioPath = request.AudioPath,
                        AnsweredAt = DateTime.UtcNow,
                        AIAnalysis = assessment.Feedback,
                        FollowUpCount = assessment.RequiresFollowUp ? 1 : 0
                    };

                    currentQuestion.Answer = answer;
                    await _unitOfWork.InterviewAnswers.AddAsync(answer); // Specialized repository

                    // Check for Follow-Up
                    if (assessment.RequiresFollowUp)
                    {
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);

                        return new SubmitInterviewAnswerResponse
                        {
                            IsCompleted = false,
                            Feedback = assessment.Feedback,
                            // Return the SAME Question ID but with the Follow-up text
                            NextQuestion = new InterviewQuestionDto
                            {
                                Id = currentQuestion.Id,
                                Text = assessment.FollowUpQuestion ?? "Bunu biraz daha detaylandırabilir misin?",
                                Order = currentQuestion.Order,
                                Type = currentQuestion.Type
                            },
                            CompletionMessage = null
                        };
                    }
                }

                // Update Database (Persistence)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Update Redis (Reflect changes)
                await _globalCache.SetAsync(cacheKey, session, TimeSpan.FromHours(2), false, cancellationToken);


                // 4. Get Next Question
                var nextQuestion = session.Questions
                    .Where(q => q.Order > currentQuestion.Order)
                    .OrderBy(q => q.Order)
                    .FirstOrDefault();

                if (nextQuestion != null)
                {
                    return new SubmitInterviewAnswerResponse
                    {
                        IsCompleted = false,
                        Feedback = feedback,
                        NextQuestion = new InterviewQuestionDto
                        {
                            Id = nextQuestion.Id,
                            Text = nextQuestion.QuestionText,
                            Order = nextQuestion.Order,
                            Type = nextQuestion.Type
                        }
                    };
                }

                // 5. Complete Interview (No more questions)
                session.CompletedAt = DateTime.UtcNow;

                // Validate context tracks session updates if we pulled from Redis and not context
                // Since we added 'answer' to context separately, we need to update session completion status
                var dbSession = await _unitOfWork.InterviewSessions.GetByIdAsync(session.Id);
                if (dbSession != null)
                {
                    dbSession.CompletedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
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

