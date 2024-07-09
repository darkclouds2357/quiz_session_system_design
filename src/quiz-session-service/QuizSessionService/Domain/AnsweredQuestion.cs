namespace QuizSessionService.Domain
{
    public class AnsweredQuestion
    {
        public string SessionQuestionId { get; set; }
        public IEnumerable<string> SubmittedAnswerIds { get; set; }
        public int Score { get; set; }
        public bool IsCorrect { get; set; }
    }
}