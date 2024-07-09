namespace QuizSessionService.Domain
{
    public class AttendedUser
    {
        private List<AnsweredQuestion> _answeredQuestions;
        public AttendedUser()
        {
            _answeredQuestions = new List<AnsweredQuestion>();
        }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime AttendedAt { get; set; }
        public int TotalScore => AnsweredQuestions.Sum(x => x.Score);
        public int Rank { get; set; }
        public IReadOnlyList<AnsweredQuestion> AnsweredQuestions => _answeredQuestions;

        public SessionQuestion NextQuestion { get; set; }

        public bool IsQuestionAnswered(string sessionQuestionId)
        {
            return AnsweredQuestions.Any(q => q.SessionQuestionId == sessionQuestionId);
        }

        internal void AnswerSubmitted(string sessionQuestionId, string[] answeredIds, int score, bool isCorrect)
        {
            _answeredQuestions.Add(new AnsweredQuestion
            {
                SessionQuestionId = sessionQuestionId,
                Score = score,
                IsCorrect = isCorrect,
                SubmittedAnswerIds = answeredIds
            });
        }

        internal void SetNextQuestion(SessionQuestion nextQuestion) => NextQuestion = nextQuestion;

        internal void UpdateRank(int rank)
        {
            Rank = rank;
        }
    }
}