namespace QuizSessionService.Domain.DomainEvents
{
    public class LeaderboardRankUpdatedEvent : DomainEvent
    {
        public LeaderboardRankUpdatedEvent(string streamId, int version) : base(Const.LEADERBOARD_RANK_UPDATED_EVENT, streamId, version)
        {
        }
        public IEnumerable<UserRank> Leaderboard { get; set; }
    }
}
