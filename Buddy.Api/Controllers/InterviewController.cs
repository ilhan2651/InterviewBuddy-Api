using Buddy.Application.Features.Interview.CompleteInterview;
using Buddy.Application.Features.Interview.GetCurrentQuestion;
using Buddy.Application.Features.Interview.GetReport;
using Buddy.Application.Features.Interview.StartInterview;
using Buddy.Application.Features.Interview.SubmitAnswer;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace Buddy.Api.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InterviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InterviewController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("start")]
        [ProducesResponseType(typeof(StartInterviewResponse), 200)]
        public async Task<IActionResult> StartInterview([FromBody] StartInterviewCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitInterviewAnswerCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteInterview([FromBody] CompleteInterviewCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("upload-audio")]
        public async Task<IActionResult> UploadAudio(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio", "user");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("audio", "user", uniqueFileName).Replace("\\", "/");
            return Ok(new { Path = relativePath });
        }

        [HttpGet("{sessionId}/current-question")]
        [ProducesResponseType(typeof(GetCurrentQuestionResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCurrentQuestion(string sessionId)
        {
            var query = new GetCurrentQuestionQuery { SessionId = sessionId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }


        [HttpGet("{sessionId}/report")]
        [ProducesResponseType(typeof(GetInterviewReportResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetReport(string sessionId)
        {
            var query = new GetInterviewReportQuery { SessionId = sessionId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
