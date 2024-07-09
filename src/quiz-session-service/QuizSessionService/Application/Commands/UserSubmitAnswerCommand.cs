
using QuizSessionService.Domain.DomainEvents;
using QuizSessionService.Dtos;

namespace QuizSessionService.Application.Commands
{
    public class UserSubmitAnswerCommand : IRequest
    {
        public string QuizSessionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public AnsweredQuestionDto Answered { get; set; }

    }
    public class UserSubmitAnswerCommandHandler : IRequestHandler<UserSubmitAnswerCommand>
    {
        private readonly Services.QuizSessionService _service;
        private readonly ILogger<UserSubmitAnswerCommandHandler> _logger;
        private readonly IMediator _mediator;

        public UserSubmitAnswerCommandHandler(Services.QuizSessionService service, ILogger<UserSubmitAnswerCommandHandler> logger, IMediator mediator)
        {

            _service = service;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Handle(UserSubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            var quizSession = await _service.GetQuizSessionAsync(request.QuizSessionId, cancellationToken);

            if (quizSession == null || !quizSession.IsValid)
            {
                await _mediator.Publish(new QuizSessionNotValidEvent(request.QuizSessionId, (quizSession?.Version ?? 0) + 1)
                {
                    AttendedUserId = request.UserId
                }, cancellationToken);
                return;
            }

            await quizSession.UserSubmitAnswerAsync(request.UserId, request.UserName, request.Answered, cancellationToken);
        }
    }
}
