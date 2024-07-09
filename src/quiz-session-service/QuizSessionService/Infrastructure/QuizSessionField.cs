namespace QuizSessionService.Infrastructure
{
    public static class QuizSessionField
    {

        public static string QuizSessionKey(string quizSessionId)
        {
            return $"quiz:session:{quizSessionId}";
        }

        public const string EVENT_STORE = "event_stores";

        public class EventStoreField
        {
            public const string STREAM_ID = "stream_id";
            public const string EVENT_NAME = "event_name";
            public const string EVENT_ASSEMBLY_TYPE = "event_assembly_type";
            public const string VERSION = "version";
            public const string CREATED_AT = "created_at";
            public const string PAYLOAD = "payload";
        }

        public const string ID = "id";
        public const string VERSION = "version";
        public const string START_TIME = "start_time";
        public const string END_TIME = "end_time";

        public const string QUESTIONS = "questions";

        public const string ATTENDED_USERS = "attended_users";

        public static class AttendedUserField
        {
            public const string ID = "id";
            public const string USER_NAME = "user_name";
            public const string SCORE = "score";
            public const string CURRENT_EXAM = "current_exam";
            public const string RANK = "rank";
            public const string PREVIOUS_RANK = "previous_rank";

            public const string ANSWERED_QUESTIONS = "answered_questions";

            public static string AnsweredQuestionMember(string questionId, string[] answeredId, bool isCorrect, int score)
            {
                return $"{questionId}:[{string.Join(",", answeredId)}]:{isCorrect}:{score}";
            }
        }

        public const string LEADERBOARD = "leaderboard";

        public static string LeaderboardMember(string userId, string userName)
        {
            return $"{userName}:{userId}";
        }
    }
}
