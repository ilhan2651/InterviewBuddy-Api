using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Interview.GetCurrentQuestion
{
    public class GetCurrentQuestionQueryHandler : IRequestHandler<GetCurrentQuestionQuery, GetCurrentQuestionResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Buddy.Application.Services.ITextToSpeechService _ttsService;
        private readonly Microsoft.Extensions.Logging.ILogger<GetCurrentQuestionQueryHandler> _logger;

        public GetCurrentQuestionQueryHandler(IUnitOfWork unitOfWork, Buddy.Application.Services.ITextToSpeechService ttsService, Microsoft.Extensions.Logging.ILogger<GetCurrentQuestionQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _ttsService = ttsService;
            _logger = logger;
        }

        public async Task<GetCurrentQuestionResponse> Handle(GetCurrentQuestionQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting current question for SessionId: {SessionId}", request.SessionId);
            var session = await _unitOfWork.InterviewSessions.GetBySessionIdAsync(request.SessionId, cancellationToken);

            if (session == null)
            {
                _logger.LogWarning("Interview session {SessionId} not found.", request.SessionId);
                throw new Exception($"Interview session {request.SessionId} not found");
            }

            // Get base questions only for counting
            var baseQuestions = session.Questions.Where(q => q.ParentId == null).OrderBy(q => q.Id).ToList();

            // Find target question if requested, otherwise find first unanswered question or last question
            InterviewQuestion currentQuestion;
            if (request.TargetQuestionNumber.HasValue)
            {
                var index = request.TargetQuestionNumber.Value - 1;
                if (index >= 0 && index < baseQuestions.Count)
                {
                    currentQuestion = baseQuestions[index];
                }
                else
                {
                    currentQuestion = session.Questions
                        .OrderBy(q => q.Id)
                        .FirstOrDefault(q => q.Answer == null) 
                        ?? session.Questions.OrderByDescending(q => q.Id).FirstOrDefault()!;
                }
            }
            else
            {
                currentQuestion = session.Questions
                    .OrderBy(q => q.Id)
                    .FirstOrDefault(q => q.Answer == null) 
                    ?? session.Questions.OrderByDescending(q => q.Id).FirstOrDefault()!;
            }

            if (currentQuestion == null)
            {
                _logger.LogWarning("No questions found for session {SessionId}.", request.SessionId);
                throw new Exception("No questions found for this session");
            }

            // Calculate DisplayNumber
            string displayNumber;
            if (currentQuestion.ParentId.HasValue)
            {
                var parentQuestion = session.Questions.FirstOrDefault(q => q.Id == currentQuestion.ParentId);
                var baseIndex = baseQuestions.IndexOf(parentQuestion!) + 1;
                // Since we only allow 1 follow-up deep currently
                displayNumber = $"{baseIndex}.1";
            }
            else
            {
                var baseIndex = baseQuestions.IndexOf(currentQuestion) + 1;
                displayNumber = baseIndex.ToString();
            }
            
            _logger.LogInformation("Resolved current question. ID: {QuestionId}, DisplayNumber: {DisplayNumber}", currentQuestion.Id, displayNumber);

            if (string.IsNullOrEmpty(currentQuestion.AudioUrl))
            {
                _logger.LogInformation("AudioUrl missing for Question {QuestionId}. Generating TTS fallback.", currentQuestion.Id);
                try
                {
                    var audioStream = await _ttsService.TextToSpeechAsync(currentQuestion.QuestionText, cancellationToken);
                    var fileName = $"q_{session.Id}_{currentQuestion.Id}_{Guid.NewGuid()}.mp3";
                    currentQuestion.AudioUrl = await _ttsService.SaveAudioAsync(audioStream, fileName, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("TTS audio successfully generated and saved for Question {QuestionId}. URL: {AudioUrl}", currentQuestion.Id, currentQuestion.AudioUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TTS Error generating fallback voice for question {QuestionId}.", currentQuestion.Id);
                }
            }
            else
            {
                _logger.LogInformation("Question {QuestionId} already has AudioUrl: {AudioUrl}", currentQuestion.Id, currentQuestion.AudioUrl);
            }

            if (!string.IsNullOrWhiteSpace(currentQuestion.AudioUrl))
            {
                var relativeAudioPath = currentQuestion.AudioUrl.Replace('/', Path.DirectorySeparatorChar);
                var absoluteAudioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeAudioPath);
                var fileExists = File.Exists(absoluteAudioPath);
                var fileSize = fileExists ? new FileInfo(absoluteAudioPath).Length : 0;
                _logger.LogInformation("Audio file diagnostics for Question {QuestionId}. RelativeUrl: {AudioUrl}, AbsolutePath: {AbsoluteAudioPath}, Exists: {FileExists}, Bytes: {FileSize}", currentQuestion.Id, currentQuestion.AudioUrl, absoluteAudioPath, fileExists, fileSize);
            }
            else
            {
                _logger.LogWarning("Question {QuestionId} still has no AudioUrl after TTS processing.", currentQuestion.Id);
            }

            return new GetCurrentQuestionResponse
            {
                Id = currentQuestion.Id,
                QuestionText = currentQuestion.QuestionText,
                QuestionNumber = 0, // Deprecated, but keeping to not break things until UI removes it fully.
                TotalQuestions = baseQuestions.Count,
                DisplayNumber = displayNumber,
                AudioUrl = currentQuestion.AudioUrl ?? string.Empty,
                ImageUrl = currentQuestion.ImageUrl,
                CodeSnippet = currentQuestion.CodeSnippet
            };
        }
    }
}
