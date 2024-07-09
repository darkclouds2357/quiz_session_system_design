
namespace QuizSessionService.Domain.DomainEvents
{
    public class QuizSessionStartedEvent : DomainEvent
    {
        public QuizSessionStartedEvent(string streamId, int version) : base(Const.QUIZ_SESSION_STARTED_EVENT, streamId, version)
        {
            SessionQuestions = Enumerable.Empty<SessionQuestion>();
        }

        public DateTime EndTime { get; set; }

        public IEnumerable<SessionQuestion> SessionQuestions { get; set; }
    }


}
