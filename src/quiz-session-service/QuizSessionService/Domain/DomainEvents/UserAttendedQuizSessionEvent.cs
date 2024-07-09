
namespace QuizSessionService.Domain.DomainEvents
{
    public class UserAttendedQuizSessionEvent : DomainEvent
    {
        public UserAttendedQuizSessionEvent(string streamId, int version) : base(Const.USER_ATTENDED_QUIZ_SESSION_EVENT, streamId, version)
        {
        }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public SessionQuestion NextQuestion { get; set; }

    }

}
