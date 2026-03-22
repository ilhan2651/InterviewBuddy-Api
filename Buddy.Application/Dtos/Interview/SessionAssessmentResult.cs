using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.Application.Dtos.Interview
{
    public class SessionAssessmentResult
    {
        public int? CommunicationScore { get; set; }
        public string? CommunicationFeedback { get; set; }
        public int? ConfidenceScore { get; set; }
        public string? ConfidenceFeedback { get; set; }
    }
}
