using System;

namespace Buddy.Application.Features.User.GetRecentInterviews
{
    public class RecentInterviewDto
    {
        public string Role { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Date { get; set; } = string.Empty;
    }
}
