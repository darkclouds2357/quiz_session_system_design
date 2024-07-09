using QuizSessionService.Domain.DomainEvents;

namespace QuizSessionService.Application.Events
{

    public class QuizSessionStartedEventHandler : INotificationHandler<QuizSessionStartedEvent>
    {
        public QuizSessionStartedEventHandler()
        {

        }

        public Task Handle(QuizSessionStartedEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
