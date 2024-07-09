namespace QuizSessionService.Domain.DomainEvents
{
    public class QuizSessionNotValidEvent : DomainEvent
    {
        public QuizSessionNotValidEvent(string streamId, int version) : base(Const.QUIZ_SESSION_NOT_VALID_EVENT, streamId, version)
        {
        }

        public string AttendedUserId { get; set; }
    }
}
