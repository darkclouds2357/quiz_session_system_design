using Newtonsoft.Json;
using QuizSessionService.Dtos;

namespace QuizSessionService.Domain
{
    public class Answer
    {
        public Answer()
        {
            Id = Guid.NewGuid().ToString();
        }
        public Answer(AnswerDto answer) : this()
        {
            Text = answer.Text;
            IsCorrectAnswer = answer.IsCorrectAnswer;
        }

        [JsonConstructor]
        public Answer(string id, string text, bool isCorrectAnswer)
        {
            Id = id;
            Text = text;
            IsCorrectAnswer = isCorrectAnswer;
        }

        public string Id { get; private set; }
        public string Text { get; private set; }
        public bool IsCorrectAnswer { get; private set; }
    }
}