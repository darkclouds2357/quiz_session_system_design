using QuizSessionService.Domain;
using QuizSessionService.Domain.DomainEvents;

namespace QuizSessionService.Infrastructure.Query
{
    public interface IQuizSessionQuery
    {
        Task ApplyQueryEventAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
        Task<LinkedList<UserRank>> GetLeaderboardAsync(string quizSessionId, CancellationToken cancellationToken = default);
    }
}
