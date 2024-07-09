namespace QuizSessionService.Dtos
{
    public class AnsweredQuestionDto
    {
        public string SessionQuestionId { get; set; }

        public string[] AnsweredIds { get; set; }
    }
}
