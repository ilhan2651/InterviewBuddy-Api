using Buddy.Application.Features.User.GetStats;
using Buddy.Application.Features.User.GetRecentInterviews;
using Buddy.Application.Features.User.GetQuotaStatus;
using Buddy.Application.Features.User.UpdateApiKeys;
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

        [HttpGet("uncompleted-interviews")]
        public async Task<IActionResult> GetUncompletedInterviews()
        {
            var query = new GetUncompletedInterviewsQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("quota-status")]
        public async Task<ActionResult<QuotaStatusResponse>> GetQuotaStatus()
        {
            var result = await _mediator.Send(new GetQuotaStatusQuery());
            return Ok(result);
        }

        [HttpPost("keys")]
        public async Task<ActionResult> UpdateApiKeys([FromBody] UpdateUserApiKeysCommand command)
        {
            var result = await _mediator.Send(command);
            if (result)
            {
                return Ok(new { message = "API anahtarları başarıyla güncellendi." });
            }
            return BadRequest(new { message = "API anahtarları güncellenirken bir hata oluştu." });
        }
    }
}
