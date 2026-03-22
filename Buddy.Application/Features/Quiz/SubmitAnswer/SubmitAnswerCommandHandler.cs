using Buddy.Application.Common.Interfaces;
using Buddy.Application.Dtos.Quiz;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Quiz.SubmitAnswer
{
    public class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, SubmitAnswerResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatLLMService _chatLLMService;

        public SubmitAnswerCommandHandler(IUnitOfWork unitOfWork, IChatLLMService chatLLMService)
        {
            _unitOfWork = unitOfWork;
            _chatLLMService = chatLLMService;
        }

        public async Task<SubmitAnswerResponse> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            // 1. Get current question and quiz info
            var currentQuestion = await _unitOfWork.QuizQuestions.GetWithQuizAsync(request.QuizQuestionId, cancellationToken);

            if (currentQuestion == null)
                throw new Exception("Soru bulunamadı.");

            var finalAnswer = request.Answer;
            string? audioPath = null;

            // 2. Handle Audio if provided
            if (request.AudioStream != null)
            {
                var audioFileName = $"answer_{request.QuizQuestionId}_{Guid.NewGuid()}.mp3";
                var relativePath = Path.Combine("audio", "user", audioFileName);
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Buddy.Api", "wwwroot", "audio", "user", audioFileName);

                var directoryPath = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath!);
                }

                using (var fileStream = File.Create(absolutePath))
                {
                    await request.AudioStream.CopyToAsync(fileStream, cancellationToken);
                }

                audioPath = relativePath.Replace("\\", "/");

                // Transcribe if text answer is missing
                if (string.IsNullOrWhiteSpace(finalAnswer))
                {
                    using var audioFileStream = File.OpenRead(absolutePath);
                    finalAnswer = await _chatLLMService.TranscribeAudioAsync(audioFileStream);
                }
            }

            // 3. Create QuizAnswer
            var quizAnswer = new QuizAnswer
            {
                QuizQuestionId = request.QuizQuestionId,
                UserAnswer = finalAnswer ?? string.Empty,
                UserAudioPath = audioPath,
                AnsweredAt = DateTime.UtcNow
            };

            await _unitOfWork.QuizAnswers.AddAsync(quizAnswer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Find next question
            var nextQuestionEntity = currentQuestion.Quiz.Questions
                .Where(qq => qq.QuestionNumber == currentQuestion.QuestionNumber + 1)
                .FirstOrDefault();

            if (nextQuestionEntity != null)
            {
                return new SubmitAnswerResponse
                {
                    QuizId = currentQuestion.QuizId,
                    HasMore = true,
                    NextQuestion = new QuizQuestionDto
                    {
                        Number = nextQuestionEntity.QuestionNumber,
                        Question = nextQuestionEntity.QuestionText
                        // We don't return ExpectedKeywords to the UI
                    }
                };
            }

            return new SubmitAnswerResponse
            {
                QuizId = currentQuestion.QuizId,
                HasMore = false,
                CompletionMessage = "Tebrikler! Sınavı tamamladınız. Değerlendirme için bekleyiniz."
            };
        }
    }
}
