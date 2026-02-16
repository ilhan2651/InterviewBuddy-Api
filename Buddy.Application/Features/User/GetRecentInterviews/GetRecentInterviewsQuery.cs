using MediatR;
using System.Collections.Generic;

namespace Buddy.Application.Features.User.GetRecentInterviews
{
    public class GetRecentInterviewsQuery : IRequest<List<RecentInterviewDto>>
    {
    }
}
