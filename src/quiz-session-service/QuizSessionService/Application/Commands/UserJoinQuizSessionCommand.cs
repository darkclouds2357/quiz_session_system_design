
using QuizSessionService.Domain.DomainEvents;

namespace QuizSessionService.Application.Commands
{
    public class UserJoinQuizSessionCommand : IRequest
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string QuizSessionId { get; set; }
    }

    public class UserJoinQuizSessionCommandHandler : IRequestHandler<UserJoinQuizSessionCommand>
    {
        private readonly Services.QuizSessionService _service;
        private readonly ILogger<UserJoinQuizSessionCommandHandler> _logger;
        private readonly IMediator _mediator;

        public UserJoinQuizSessionCommandHandler(Services.QuizSessionService service, ILogger<UserJoinQuizSessionCommandHandler> logger, IMediator mediator)
        {
            _service = service;
            _logger = logger;
            _mediator = mediator;
        }
        public async Task Handle(UserJoinQuizSessionCommand request, CancellationToken cancellationToken)
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

            await quizSession.UserJoinQuizSessionAsync(request.UserId, request.UserName, cancellationToken);
        }
    }
}
