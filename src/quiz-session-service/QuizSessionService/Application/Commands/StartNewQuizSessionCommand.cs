

using QuizSessionService.Dtos;

namespace QuizSessionService.Application.Commands
{
    public class StartNewQuizSessionCommand : IRequest
    {
        public DateTime EndTime { get; set; }
        public QuestionDto[] Questions { get; set; }
    }

    public class StartNewQuizSessionCommandHandler : IRequestHandler<StartNewQuizSessionCommand>
    {
        private readonly Services.QuizSessionService _service;
        private readonly ILogger<StartNewQuizSessionCommandHandler> _logger;
        private readonly IMediator _mediator;

        public StartNewQuizSessionCommandHandler(Services.QuizSessionService service, ILogger<StartNewQuizSessionCommandHandler> logger, IMediator mediator)
        {
            this._service = service;
            this._logger = logger;
            this._mediator = mediator;
        }

        public async Task Handle(StartNewQuizSessionCommand request, CancellationToken cancellationToken)
        {
            var quizSession = await _service.GetQuizSessionAsync(cancellationToken: cancellationToken);

            await quizSession.QuizStartAsync(request.EndTime, request.Questions, cancellationToken);
        }
    }
}
