using QuizSessionService.Domain;

namespace QuizSessionService.Dtos
{
    public class UserQuizSessionDto
    {
        public string UserId { get; set; }
        public int? Score { get; set; }
        public int? Rank { get; set; }

        public UserQuizQuestionDto Question { get; set; }
    }

    public class UserQuizQuestionDto
    {
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public string Text { get; set; }
        public int Score { get; set; }
        public string QuestionType { get; set; }

        public IEnumerable<UserQuizAnswerDto> Answers { get; set; }
    }
    public class UserQuizAnswerDto
    {
        public string Text { get; set; }
        public string Id { get; set; }
    }
}
