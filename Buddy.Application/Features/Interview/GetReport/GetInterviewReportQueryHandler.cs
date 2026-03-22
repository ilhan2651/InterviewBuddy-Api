using Buddy.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
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
            var session = await _unitOfWork.InterviewSessions.GetBySessionIdAsync(request.SessionId, cancellationToken);

            if (session == null)
            {
                throw new Exception($"Interview session {request.SessionId} not found");
            }

            var questionAnswers = session.Questions
                .Where(q => q.Answer != null)
                .Select(q => new QuestionAnswerDto
                {
                    Question = q.QuestionText,
                    CodeSnippet = q.CodeSnippet,
                    UserAnswer = q.Answer!.UserAnswerText ?? "Cevap metni bulunamadi",
                    AiFeedback = q.Answer.AIAnalysis ?? "Henuz degerlendirme yapilmadi",
                    Score = q.Answer.Score ?? 0,
                    IdealAnswerSummary = q.Answer.IdealAnswerSummary
                })
                .ToList();

            if (!questionAnswers.Any())
            {
                return new GetInterviewReportResponse
                {
                    OverallScore = 0,
                    TechnicalScore = 0,
                    CommunicationScore = 0,
                    ConfidenceScore = 0,
                    QuestionAnswers = questionAnswers,
                    Strengths = new List<string> { "Mulakat tamamlanmamis" },
                    ImprovementAreas = new List<string> { "Mulakat tamamlanmamis" },
                    ImprovmentArea = new List<string> { "Mulakat tamamlanmamis" },
                    Recommendations = new List<string> { "Mulakat tamamlanmamis" }
                };
            }

            var overallScore = ClampScore(questionAnswers.Average(qa => qa.Score));
            var technicalScore = overallScore;
            var communicationScore = session.CommunicationScore ?? overallScore;
            var confidenceScore = session.ConfidenceScore ?? overallScore;

            var strengths = GenerateStrengths(technicalScore, communicationScore, confidenceScore);
            var improvementAreas = GenerateImprovementAreas(technicalScore, communicationScore, confidenceScore);
            var recommendations = GenerateRecommendations(overallScore, technicalScore, communicationScore, confidenceScore);

            return new GetInterviewReportResponse
            {
                OverallScore = overallScore,
                TechnicalScore = technicalScore,
                CommunicationScore = communicationScore,
                ConfidenceScore = confidenceScore,
                QuestionAnswers = questionAnswers,
                Recommendations = recommendations,
                Strengths = strengths,
                ImprovementAreas = improvementAreas,
                ImprovmentArea = improvementAreas
            };
        }

        private List<string> GenerateRecommendations(int overall, int technical, int communication, int confidence)
        {
            var recommendations = new List<string>();

            if (technical < 70)
            {
                recommendations.Add("Teknik bilgini gelistirmek icin ilgili konularda daha fazla pratik yapmalisin");
            }

            if (communication < 70)
            {
                recommendations.Add("Cevaplarinda daha net ve yapilandirilmis bir anlatim kullanmaya calis");
            }

            if (confidence < 70)
            {
                recommendations.Add("Kendine guvenini artirmak icin daha fazla mulakat pratigi yapabilirsin");
            }

            if (overall >= 80)
            {
                recommendations.Add("Harika bir performans! Gercek mulakata hazirsin");
            }
            else if (overall >= 60)
            {
                recommendations.Add("Iyi bir baslangic, birkac pratikle daha da iyilestirebilirsin");
            }
            else
            {
                recommendations.Add("Daha fazla hazirlik ve pratik yapman gerekiyor");
            }

            return recommendations;
        }

        private static int ClampScore(double score)
        {
            return Math.Max(0, Math.Min(100, (int)Math.Round(score)));
        }

        private List<string> GenerateStrengths(int technical, int communication, int confidence)
        {
            var strengths = new List<string>();

            if (technical >= 75)
            {
                strengths.Add("Teknik bilgi ve problem cozme tarafi guclu gorunuyor");
            }

            if (communication >= 75)
            {
                strengths.Add("Kendini acik ve anlasilir ifade edebiliyorsun");
            }

            if (confidence >= 75)
            {
                strengths.Add("Sunum ve genel ozguven tarafinda guclu bir izlenim birakiyorsun");
            }

            if (!strengths.Any())
            {
                strengths.Add("Temel mulakat akisini surdurebilecek bir baslangic seviyen var");
            }

            return strengths;
        }

        private List<string> GenerateImprovementAreas(int technical, int communication, int confidence)
        {
            var improvementAreas = new List<string>();

            if (technical < 70)
            {
                improvementAreas.Add("Teknik cevaplarda daha derin, ornekli ve yapilandirilmis aciklamalar vermeye odaklan");
            }

            if (communication < 70)
            {
                improvementAreas.Add("Cevaplarini daha net, kisa ve duzenli parcalara ayirarak anlatmayi dene");
            }

            if (confidence < 70)
            {
                improvementAreas.Add("Daha fazla pratik yaparak ses tonu, akicilik ve genel sunum tarafini guclendirebilirsin");
            }

            if (!improvementAreas.Any())
            {
                improvementAreas.Add("Su an icin belirgin bir zayif alan gorunmuyor, mevcut performansini koruyacak sekilde pratik yapmaya devam et");
            }

            return improvementAreas;
        }
    }
}
