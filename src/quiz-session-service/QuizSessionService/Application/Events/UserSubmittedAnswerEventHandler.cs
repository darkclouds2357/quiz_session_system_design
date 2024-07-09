using QuizSessionService.Application.Commands;
using QuizSessionService.Domain.DomainEvents;
using QuizSessionService.Dtos;
using QuizSessionService.MessageBus;

namespace QuizSessionService.Application.Events
{

    public class UserSubmittedAnswerEventHandler : INotificationHandler<UserSubmittedAnswerEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public UserSubmittedAnswerEventHandler(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
        public async Task Handle(UserSubmittedAnswerEvent @event, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            await mediator.Send(new CalculateUsersRankCommand
            {
                QuizSessionId = @event.StreamId
            }, cancellationToken);


            var nextQuestion = default(UserQuizQuestionDto);

            if (@event.NextQuestion != null)
            {
                nextQuestion = new UserQuizQuestionDto
                {
                    Id = @event.NextQuestion.Id,
                    QuestionId = @event.NextQuestion.QuestionId,
                    QuestionType = @event.NextQuestion.QuestionType,
                    Score = @event.NextQuestion.Score,
                    Text = @event.NextQuestion.Text,
                    Answers = @event.NextQuestion.Answers.Select(c => new UserQuizAnswerDto
                    {
                        Id = c.Id,
                        Text = c.Text,
                    }).OrderBy(c => Guid.NewGuid())
                };
            }

            /// get message bus 
            await messageBus.PublishAsync(Const.PUSH_TO_NOTIFIER_COMMAND, new PushToNotifierCommand
            {
                Channel = Const.GetUserQuizSessionChannel(@event.UserId, @event.StreamId),
                EventName = @event.EventName,
                CreatedAt = @event.CreatedAt,
                Payload = new UserQuizSessionDto
                {
                    Question = nextQuestion,
                    UserId = @event.UserId,
                    Score = @event.CurrentScore
                }
            }, cancellationToken);

        }
    }
}
