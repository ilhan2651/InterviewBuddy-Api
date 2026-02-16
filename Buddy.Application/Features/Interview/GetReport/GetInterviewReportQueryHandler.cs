using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Interview.GetReport
{
    public class GetInterviewReportQueryHandler : IRequestHandler<GetInterviewReportQuery, GetInterviewReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetInterviewReportQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetInterviewReportResponse> Handle(GetInterviewReportQuery request, CancellationToken cancellationToken)
        {
            var session = await _unitOfWork.InterviewSessions.GetWithDetailsAsync(request.SessionId, cancellationToken);

            if (session == null)
            {
                throw new Exception($"Interview session {request.SessionId} not found");
            }

            var questionAnswers = session.Questions
                .Where(q => q.Answer != null)
                .Select(q => new QuestionAnswerDto
                {
                    Question = q.QuestionText,
                    UserAnswer = q.Answer!.UserAnswerText ?? "Cevap metni bulunamadı",
                    AiFeedback = q.Answer.AIAnalysis ?? "Henüz değerlendirme yapılmadı",
                    Score = q.Answer.Score ?? 0
                })
                .ToList();

            // Calculate category scores
            var totalQuestions = questionAnswers.Count;
            if (totalQuestions == 0)
            {
                return new GetInterviewReportResponse
                {
                    OverallScore = 0,
                    TechnicalScore = 0,
                    CommunicationScore = 0,
                    ConfidenceScore = 0,
                    QuestionAnswers = questionAnswers,
                    Recommendations = new List<string> { "Mülakat tamamlanmamış" }
                };
            }

            var overallScore = (int)questionAnswers.Average(qa => qa.Score);
            
            // Mock category scores (ideally these would be calculated based on question types)
            var technicalScore = Math.Min(100, overallScore + new Random().Next(-10, 15));
            var communicationScore = Math.Min(100, overallScore + new Random().Next(-15, 10));
            var confidenceScore = Math.Min(100, overallScore + new Random().Next(-10, 10));

            // Generate recommendations
            var recommendations = GenerateRecommendations(overallScore, technicalScore, communicationScore, confidenceScore);

            return new GetInterviewReportResponse
            {
                OverallScore = overallScore,
                TechnicalScore = technicalScore,
                CommunicationScore = communicationScore,
                ConfidenceScore = confidenceScore,
                QuestionAnswers = questionAnswers,
                Recommendations = recommendations
            };
        }

        private List<string> GenerateRecommendations(int overall, int technical, int communication, int confidence)
        {
            var recommendations = new List<string>();

            if (technical < 70)
            {
                recommendations.Add("Teknik bilgini geliştirmek için ilgili konularda daha fazla pratik yapmalısın");
            }

            if (communication < 70)
            {
                recommendations.Add("Cevaplarında daha net ve yapılandırılmış bir anlatım kullanmaya çalış");
            }

            if (confidence < 70)
            {
                recommendations.Add("Kendine güvenini artırmak için daha fazla mülakat pratiği yapabilirsin");
            }

            if (overall >= 80)
            {
                recommendations.Add("Harika bir performans! Gerçek mülakata hazırsın");
            }
            else if (overall >= 60)
            {
                recommendations.Add("İyi bir başlangıç, birkaç pratikle daha da iyileştirebilirsin");
            }
            else
            {
                recommendations.Add("Daha fazla hazırlık ve pratik yapman gerekiyor");
            }

            return recommendations;
        }
    }
}
