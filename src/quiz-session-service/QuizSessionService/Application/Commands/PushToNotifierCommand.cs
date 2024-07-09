namespace QuizSessionService.Application.Commands
{
    public class PushToNotifierCommand
    {
        public string Channel { get; set; }

        public object Payload { get; set; }
        public string EventName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
