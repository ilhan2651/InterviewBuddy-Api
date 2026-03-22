using Buddy.Application.Common.Interfaces;
using Buddy.Application.Dtos.Quiz;
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

namespace Buddy.Application.Features.Quiz.CompleteQuiz
{
    public class CompleteQuizCommandHandler : IRequestHandler<CompleteQuizCommand, QuizResultResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQuizLLMService _quizLLMService;

        public CompleteQuizCommandHandler(IUnitOfWork unitOfWork, IQuizLLMService quizLLMService)
        {
            _unitOfWork = unitOfWork;
            _quizLLMService = quizLLMService;
        }

        public async Task<QuizResultResponse> Handle(CompleteQuizCommand request, CancellationToken cancellationToken)
        {
            // 1. Load Quiz with all questions and answers
            var quiz = await _unitOfWork.Quizzes.GetWithQuestionsAsync(request.QuizId, cancellationToken);

            if (quiz == null)
                throw new Exception("Sınav bulunamadı.");

            if (quiz.Status == QuizStatus.Completed)
                throw new Exception("Sınav zaten tamamlanmış.");

            // 2. Prepare inputs for AI Evaluation
            var evalInputs = quiz.Questions
                .Where(qq => qq.Answer != null)
                .Select(qq => new QuizEvaluationInput
                {
                    QuestionNumber = qq.QuestionNumber,
                    Question = qq.QuestionText,
                    UserAnswer = qq.Answer!.UserAnswer,
                    ExpectedKeywords = string.IsNullOrWhiteSpace(qq.ExpectedKeywords) 
                        ? new List<string>() 
                        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(qq.ExpectedKeywords) ?? new List<string>()
                })
                .ToList();

            if (!evalInputs.Any())
            {
                quiz.Status = QuizStatus.Abandoned;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new QuizResultResponse { Summary = "Hiç cevap verilmediği için sınav iptal edildi." };
            }

            // 3. Call AI for Evaluation
            var evaluationResult = await _quizLLMService.EvaluateQuizAsync(evalInputs);

            // 4. Update Database
            foreach (var eval in evaluationResult.Evaluations)
            {
                var question = quiz.Questions.FirstOrDefault(qq => qq.QuestionNumber == eval.QuestionNumber);
                if (question?.Answer != null)
                {
                    question.Answer.Score = eval.Score;
                    question.Answer.Feedback = eval.Feedback;
                    question.Answer.EvaluatedAt = DateTime.UtcNow;
                }
            }

            quiz.Status = QuizStatus.Completed;
            quiz.TotalScore = evaluationResult.TotalScore;
            quiz.FeedbackSummary = evaluationResult.Summary;
            quiz.CompletedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Build Response
            return new QuizResultResponse
            {
                TotalScore = quiz.TotalScore ?? 0,
                Summary = quiz.FeedbackSummary ?? string.Empty,
                Details = quiz.Questions
                    .OrderBy(qq => qq.QuestionNumber)
                    .Select(qq => new QuestionResult
                    {
                        QuestionNumber = qq.QuestionNumber,
                        Question = qq.QuestionText,
                        UserAnswer = qq.Answer?.UserAnswer ?? "Cevap yok",
                        Score = qq.Answer?.Score ?? 0,
                        Feedback = qq.Answer?.Feedback ?? string.Empty
                    })
                    .ToList()
            };
        }
    }
}
