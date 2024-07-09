using QuizSessionService.Application.Commands;
using QuizSessionService.Domain;
using QuizSessionService.Domain.DomainEvents;
using QuizSessionService.Dtos;
using QuizSessionService.Infrastructure.Query;
using QuizSessionService.MessageBus;

namespace QuizSessionService.Application.Events
{
    public class UserRankChangedEventHandler : INotificationHandler<UserRankChangedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public UserRankChangedEventHandler(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
        public async Task Handle(UserRankChangedEvent @event, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            await Task.WhenAll(
            [
                // publish new user rank to message bus for private channel
                messageBus.PublishAsync(Const.PUSH_TO_NOTIFIER_COMMAND, new PushToNotifierCommand
                {
                    Channel = Const.GetUserQuizSessionChannel(@event.UserId, @event.StreamId),
                    EventName = @event.EventName,
                    CreatedAt = @event.CreatedAt,
                    Payload = new UserQuizSessionDto
                    {
                        UserId = @event.UserId,
                        Rank = @event.NewRank,
                    }
                }, cancellationToken)
            ]);

        }
    }
}
