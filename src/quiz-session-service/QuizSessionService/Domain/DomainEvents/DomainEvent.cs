namespace QuizSessionService.Domain.DomainEvents
{
    public abstract class DomainEvent : INotification, IDomainEvent
    {
        public DomainEvent(string eventName, string streamId, int version)
        {
            EventId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            EventName = eventName;
            StreamId = streamId;
            Version = version;
        }
        public string EventId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EventName { get; set; }

        public string StreamId { get; set; }
        public int Version { get; set; }
    }
}
