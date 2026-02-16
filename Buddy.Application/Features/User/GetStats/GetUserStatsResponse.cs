namespace Buddy.Application.Features.User.GetStats
{
    public class GetUserStatsResponse
    {
        public int TotalInterviews { get; set; }
        public int TechnicalScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ConfidenceScore { get; set; }
    }
}
