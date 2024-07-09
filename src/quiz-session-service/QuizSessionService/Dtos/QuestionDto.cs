using QuizSessionService.Domain;

namespace QuizSessionService.Dtos
{
    public class QuestionDto
    {
        public string QuestionId { get; set; }
        public string Text { get; set; }
        public int Score { get; set; }
        public string QuestionType { get; set; }

        public IEnumerable<AnswerDto> Answers { get; set; }
    }
}
