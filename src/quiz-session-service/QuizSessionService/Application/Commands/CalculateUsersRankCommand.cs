

using Microsoft.Extensions.Logging;
using QuizSessionService.Domain.DomainEvents;
using QuizSessionService.Infrastructure.Query;
using QuizSessionService.MessageBus;

namespace QuizSessionService.Application.Commands
{
    public class CalculateUsersRankCommand : IRequest
    {
        public string QuizSessionId { get; set; }
    }

    public class CalculateUsersRankCommandHandler : IRequestHandler<CalculateUsersRankCommand>
    {
        private readonly Services.QuizSessionService _service;
        private readonly ILogger<UserJoinQuizSessionCommandHandler> _logger;
        private readonly IMediator _mediator;

        public CalculateUsersRankCommandHandler(Services.QuizSessionService service, ILogger<UserJoinQuizSessionCommandHandler> logger, IMediator mediator)
        {
            this._service = service;
            this._logger = logger;
            this._mediator = mediator;
        }
        public async Task Handle(CalculateUsersRankCommand request, CancellationToken cancellationToken)
        {
            var quizSession = await _service.GetQuizSessionAsync(request.QuizSessionId, cancellationToken);

            if (quizSession == null || !quizSession.IsValid)
            {
                await _mediator.Publish(new QuizSessionNotValidEvent(request.QuizSessionId, (quizSession?.Version ?? 0) + 1), cancellationToken);
                return;
            }

            await quizSession.CalculateUsersRankAsync(cancellationToken);

        }
    }
}
