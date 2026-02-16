using Buddy.Application.Features.User.GetStats;
using Buddy.Application.Features.User.GetRecentInterviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Buddy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var query = new GetUserStatsQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("recent-interviews")]
        public async Task<IActionResult> GetRecentInterviews()
        {
            var query = new GetRecentInterviewsQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
