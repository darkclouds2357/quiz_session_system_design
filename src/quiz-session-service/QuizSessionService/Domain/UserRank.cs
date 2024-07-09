namespace QuizSessionService.Domain
{
    public class UserRank
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }
    }

    public static class LeaderboardExtension
    {

        // TODO this is simple version using iterate
        // this can be the problem with large number record in session leaderboard
        // in the real impl, should use zset in redis
        public static IEnumerable<UserRank> UpdateRank(this IEnumerable<UserRank> leaderboards)
        {
            var result = leaderboards.OrderByDescending(x => x.Score).ToList();

            for (var i = 0; i < result.Count; i++)
            {
                result[i].Rank = i + 1;
            }

            return result;

        }
    }
}