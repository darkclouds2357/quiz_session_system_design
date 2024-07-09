using QuizSessionService.Application.Commands;
using QuizSessionService.Domain.DomainEvents;
using QuizSessionService.Infrastructure.Query;
using QuizSessionService.MessageBus;

namespace QuizSessionService.Application.Events
{
    public class LeaderboardRankUpdatedEventHandler : INotificationHandler<LeaderboardRankUpdatedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public LeaderboardRankUpdatedEventHandler(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public async Task Handle(LeaderboardRankUpdatedEvent request, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var query = scope.ServiceProvider.GetRequiredService<IQuizSessionQuery>();
            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var leaderboard = await query.GetLeaderboardAsync(request.StreamId, cancellationToken);
            await messageBus.PublishAsync(Const.PUSH_TO_NOTIFIER_COMMAND, new PushToNotifierCommand
            {
                Channel = Const.GetQuizSessionLeaderboardChannel(request.StreamId),
                Payload = leaderboard.ToList(),
                EventName = request.EventName,
                CreatedAt = request.CreatedAt,
            }, cancellationToken);

        }
    }
}
