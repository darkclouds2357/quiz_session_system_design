using Microsoft.Extensions.Logging;
using QuizSessionService.Dtos;
using System.Threading.Channels;

namespace QuizSessionService
{
    public class Const
    {
        public const string QUIZ_SESSION_STARTED_EVENT = "quiz-session-started-event";
        public const string QUIZ_SESSION_NOT_VALID_EVENT = "quiz-session-not-valid-event";
        public const string USER_ATTENDED_QUIZ_SESSION_EVENT = "user-attended-quiz-session-event";
        public const string USER_SUBMITTED_ANSWER_EVENT = "user-submitted-answer-event";
        public const string USER_RANK_CHANGED_EVENT = "user-rank-changed-event";
        public const string LEADERBOARD_RANK_UPDATED_EVENT = "leaderboard-rank-updated-event";

        public const string PUSH_TO_NOTIFIER_COMMAND = "push-to-notifier-command";
        public const string START_NEW_QUIZ_SESSION_COMMAND = "start-new-quiz-session-command";

        public static string GetUserQuizSessionChannel(string userId, string quizSessionId)
        {
            return $"user/{userId}/quiz/session/{quizSessionId}";
        }

        public static string GetQuizSessionLeaderboardChannel(string quizSessionId)
        {
            return $"quiz/session/{quizSessionId}/leaderboard";
        }
    }
}
