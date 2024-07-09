namespace NotifierWorker
{
    public interface IMessageBus
    {
        Task SubscribeAsync(string messageName, Type type, CancellationToken cancellationToken = default);

    }
}
