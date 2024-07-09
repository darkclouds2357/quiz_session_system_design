using Asp.Versioning;
using Microsoft.AspNetCore.SignalR;
using QuizSessionService.Application.Commands;
using QuizSessionService.Dtos;
using QuizSessionService.Dtos.Requests;

namespace QuizSessionService.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v1/quiz")]
    public class QuizSessionController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuizSessionController> _logger;

        public QuizSessionController(IServiceProvider serviceProvider, ILogger<QuizSessionController> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="user">This is only for POC, in real product should get user id from jwt and claim header</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("session/{sessionId}/join")]
        public async Task<IActionResult> UserJoinQuizSessionAsync(string sessionId, [FromBody] User user, CancellationToken cancellationToken = default)
        {
            if (user == null) // Validate flow for user is valid to join quiz session or not
            {
                return BadRequest();
            }

            await Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetService<IMediator>();

                await mediator.Send(new UserJoinQuizSessionCommand
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    QuizSessionId = sessionId
                }, cancellationToken);

            }, cancellationToken).ConfigureAwait(false);

            return Accepted();
        }

        [HttpPatch("session/{sessionId}/exam/{questionId}")]
        public async Task<IActionResult> UserSubmitAnswersAsync(string sessionId, string questionId, [FromBody] UserExam request, CancellationToken cancellationToken = default)
        {
            await Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetService<IMediator>();

                await mediator.Send(new UserSubmitAnswerCommand
                {
                    UserId = request.User.UserId,
                    UserName = request.User.UserName,
                    QuizSessionId = sessionId,
                    Answered = new AnsweredQuestionDto
                    {
                        AnsweredIds = request.AnsweredIds,
                        SessionQuestionId = questionId
                    }
                }, cancellationToken);

            }, cancellationToken).ConfigureAwait(false);

            return Accepted();
        }
    }
}
