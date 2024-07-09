
namespace QuizSessionService.Domain.DomainEvents
{
    public class UserSubmittedAnswerEvent : DomainEvent
    {
        public UserSubmittedAnswerEvent(string streamId, int version) : base(Const.USER_SUBMITTED_ANSWER_EVENT, streamId, version)
        {

        }

        public SessionQuestion NextQuestion { get; set; }

        public int QuestionScore { get; set; }
        public int CurrentScore { get; set; }
        public bool IsCorrect { get; set; }
        public string SessionQuestionId { get; set; }
        public string[] AnsweredIds { get; set; }
        public string UserId { get; set; }
    }

}
