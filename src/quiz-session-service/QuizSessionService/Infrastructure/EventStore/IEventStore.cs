using QuizSessionService.Domain.DomainEvents;

namespace QuizSessionService.Infrastructure.EventStore
{
    public interface IEventStore
    {
        public Task AddEventAsync(string streamId, IDomainEvent @event, CancellationToken cancellationToken = default);
        public Task<IEnumerable<IDomainEvent>> GetEventsAsync(string streamId, int fromVersion = 0, CancellationToken cancellationToken = default);
    }
}
