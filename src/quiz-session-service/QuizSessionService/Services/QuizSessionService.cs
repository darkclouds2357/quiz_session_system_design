using Microsoft.CSharp.RuntimeBinder;
using QuizSessionService.Domain;
using QuizSessionService.Infrastructure.EventStore;

namespace QuizSessionService.Services
{
    public class QuizSessionService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventStore _eventStoreRepository;

        public QuizSessionService(IServiceProvider serviceProvider, ILogger<QuizSessionService> logger, IEventStore eventStoreRepository)
        {
            this._serviceProvider = serviceProvider;
            this._eventStoreRepository = eventStoreRepository;
        }


        public async Task<QuizSession> GetQuizSessionAsync(string sessionId = null, CancellationToken cancellationToken = default)
        {
            var quizSession = _serviceProvider.GetService<QuizSession>();

            if (!string.IsNullOrWhiteSpace(sessionId))
            {

                var events = await _eventStoreRepository.GetEventsAsync(sessionId, 0, cancellationToken);

                foreach (var @event in events)
                {
                    try
                    {
                        ((dynamic)quizSession).Apply((dynamic)@event);

                    }
                    catch (RuntimeBinderException)
                    {
                    }
                    catch (MissingMethodException)
                    {
                    }
                }
            }


            return quizSession;

        }
    }
}
