
namespace QuizSessionService.Domain.DomainEvents
{
    public class UserRankChangedEvent : DomainEvent
    {
        public UserRankChangedEvent(string streamId, int version) : base(Const.USER_RANK_CHANGED_EVENT, streamId, version)
        {
        }

        public int NewRank { get; set; }
        public int PreviousRank { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
    }

}
