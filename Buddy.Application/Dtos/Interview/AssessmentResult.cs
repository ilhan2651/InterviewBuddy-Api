namespace Buddy.Application.Dtos.Interview
{
    public class AssessmentResult
    {
        public string Feedback { get; set; } = string.Empty;
        public bool RequiresFollowUp { get; set; }
        public string? FollowUpQuestion { get; set; }
        public int Score { get; set; }
    }
}
