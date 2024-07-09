namespace QuizSessionService.Domain.DomainEvents
{
    public interface IDomainEvent
    {
        string EventId { get; set; }
        DateTime CreatedAt { get; set; }
        string EventName { get; set; }

        string StreamId { get; set; }
        int Version { get; set; }
    }
}
