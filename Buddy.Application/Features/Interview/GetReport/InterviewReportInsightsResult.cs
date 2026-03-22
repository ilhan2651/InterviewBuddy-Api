using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.Application.Features.Interview.GetReport
{
    public class InterviewReportInsightsResult
    {
        public string IdealAnswerSummary { get; set; }=string.Empty;
        public List<string> NextStepRecomendations { get; set; } = new();
    }
}
