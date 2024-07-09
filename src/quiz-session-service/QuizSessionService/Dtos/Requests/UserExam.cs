namespace QuizSessionService.Dtos.Requests
{
    public class UserExam
    {
        public User User { get; set; }

        public string[] AnsweredIds { get; set; }
    }
}
