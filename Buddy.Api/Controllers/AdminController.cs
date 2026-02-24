using Buddy.Application.Features.Admin.GetSessionDetails;
using Buddy.Application.Features.Admin.GetSessions;
using Buddy.Application.Features.Admin.GetUsers;
using Buddy.Application.Features.Admin.ReEvaluateAnswer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Buddy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] - Keep authorize if role logic exists, else open for now since it's an internal test tool as per requirement
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _mediator.Send(new GetAdminUsersQuery());
            return Ok(result);
        }

        [HttpGet("users/{userId}/sessions")]
        public async Task<IActionResult> GetSessionsForUser(int userId)
        {
            var result = await _mediator.Send(new GetAdminSessionsQuery { UserId = userId });
            return Ok(result);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSessionDetails(int sessionId)
        {
            var result = await _mediator.Send(new GetAdminSessionDetailsQuery { SessionId = sessionId });
            return Ok(result);
        }

        [HttpPost("answers/{answerId}/reevaluate")]
        public async Task<IActionResult> ReEvaluateAnswer(int answerId, [FromBody] ReEvaluateAnswerCommand command)
        {
            command.AnswerId = answerId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
