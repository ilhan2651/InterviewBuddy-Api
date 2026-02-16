using Buddy.Application.Features.Chat.GetHistory;
using Buddy.Application.Features.Chat.SendAudioMessage;
using Buddy.Application.Features.Chat.SendTextMessage;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Buddy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("send-text")]
        public async Task<ActionResult<SendTextMessageResponse>> SendText([FromBody] SendTextMessageCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }


        [HttpPost("send-audio")]
        [ApiExplorerSettings(IgnoreApi = true)] // Exclude from Swagger due to complex form handling
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<ActionResult<SendTextMessageResponse>> SendAudio(
            [FromForm] IFormFile file, 
            [FromForm] string? anonymousId, 
            [FromForm] string? sessionId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Ses dosyası bulunamadı.");
            }

            var command = new SendAudioMessageCommand
            {
                AnonymousId = anonymousId,
                SessionId = sessionId,
                AudioStream = file.OpenReadStream(),
                FileName = file.FileName
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("history/{sessionId}")]
        public async Task<ActionResult<List<ChatHistoryResponse>>> GetHistory(string sessionId)
        {
            var query = new GetChatHistoryQuery { SessionId = sessionId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
