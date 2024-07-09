using Newtonsoft.Json;
using QuizSessionService.Dtos;

namespace QuizSessionService.Domain
{
    public class SessionQuestion
    {
        public SessionQuestion()
        {
            Answers = Enumerable.Empty<Answer>();
            Id = Guid.NewGuid().ToString();
        }

        public SessionQuestion(QuestionDto questionDto) : this()
        {
            Id = questionDto.QuestionId;
            QuestionId = questionDto.QuestionId;
            QuestionType = questionDto.QuestionType;
            Text = questionDto.Text;
            Score = questionDto.Score;
            Answers = questionDto.Answers.Select(a => new Answer(a)).ToList();
        }

        [JsonConstructor]
        public SessionQuestion(string id, string questionId, string text, int score, string questionType, IEnumerable<Answer> answers) : this()
        {
            Id = id;
            QuestionId = questionId;
            Text = text;
            Score = score;
            QuestionType = questionType;
            Answers = answers;
        }

        public string Id { get; private set; }
        public string QuestionId { get; private set; }
        public string Text { get; private set; }
        public int Score { get; private set; }
        public string QuestionType { get; private set; }

        public IEnumerable<Answer> Answers { get; private set; }


        private IEnumerable<Answer> _correctAnswers => Answers.Where(a => a.IsCorrectAnswer);

        public bool ValidateAnswer(string[] answerIds, out int score)
        {
            score = 0;
            // TODO should have logic validate base on QuestionType
            // can apply by call other validate answer module
            // current just use with simple logic

            var result = answerIds.Length == _correctAnswers.Count() && answerIds.All(id => _correctAnswers.Any(a => a.Id == id));


            if (result)
            {
                score = Score;
            }

            return result;

        }
    }

    public static class SessionQuestionExtension
    {
        public static SessionQuestion FirstRandomValue(this IEnumerable<SessionQuestion> sessionQuestions)
        {
            return sessionQuestions.OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }
    }
}