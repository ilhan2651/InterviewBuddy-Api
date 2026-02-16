using Buddy.Application.Features.Quiz.CompleteQuiz;
using Buddy.Application.Features.Quiz.StartQuiz;
using Buddy.Application.Features.Quiz.SubmitAnswer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Buddy.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuizController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("start")]
        public async Task<ActionResult<StartQuizResponse>> Start([FromBody] StartQuizCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("answer")]
        public async Task<ActionResult<object>> Answer([FromForm] int quizQuestionId, [FromForm] string? answer, IFormFile? file)
        {
            var command = new SubmitAnswerCommand(
                quizQuestionId,
                answer ?? string.Empty,
                file?.OpenReadStream());

            var response = await _mediator.Send(command);

            // Auto-complete if finished
            if (!response.HasMore)
            {
                var completionResult = await _mediator.Send(new CompleteQuizCommand(response.QuizId));
                return Ok(new 
                { 
                    Progress = response, 
                    Result = completionResult 
                });
            }

            return Ok(response);
        }

        [HttpGet("result/{quizId}")]
        public async Task<ActionResult<QuizResultResponse>> GetResult(int quizId)
        {
            var response = await _mediator.Send(new CompleteQuizCommand(quizId));
            return Ok(response);
        }
    }
}
