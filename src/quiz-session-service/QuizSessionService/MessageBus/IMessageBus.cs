using Microsoft.AspNetCore.Http.Headers;

namespace QuizSessionService.MessageBus
{
    public interface IMessageBus
    {
        Task PublishAsync(string messageName, object message, CancellationToken cancellationToken = default);
        Task SubscribeAsync(string messageName, Type type, CancellationToken cancellationToken = default);
    }
}
