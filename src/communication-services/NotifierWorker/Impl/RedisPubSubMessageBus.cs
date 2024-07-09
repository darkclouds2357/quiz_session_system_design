
using Newtonsoft.Json;
using NotifierWorker.Application.Commands;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace NotifierWorker.Impl
{
    public class RedisPubSubMessageBus : IMessageBus
    {
        private readonly IRedisClientFactory _clientFactory;
        private readonly IServiceProvider _serviceProvider;
        public RedisPubSubMessageBus(IRedisClientFactory clientFactory, IServiceProvider serviceProvider)
        {
            this._clientFactory = clientFactory;
            this._serviceProvider = serviceProvider;
        }
        public async Task SubscribeAsync(string messageName, Type type, CancellationToken cancellationToken = default)
        {
            var multiplexer = _clientFactory.GetDefaultRedisClient().ConnectionPoolManager.GetConnection();

            await multiplexer.GetSubscriber().SubscribeAsync(messageName, async (_, value) =>
            {
                using var scope = _serviceProvider.CreateScope();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var payload = JsonConvert.DeserializeObject(value, type);

                if (payload != null)
                    await mediator.Send(payload);

            }, flags: CommandFlags.PreferReplica);
        }
    }
    public class RedisSubscribeService : BackgroundService, IHostedService
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<RedisSubscribeService> _logger;

        public RedisSubscribeService(IMessageBus messageBus, ILogger<RedisSubscribeService> logger)
        {
            this._messageBus = messageBus;
            this._logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var messageTypes = new Dictionary<string, Type>()
                {
                    [Const.PUSH_TO_NOTIFIER_COMMAND] = typeof(PushToNotifierCommand)
                };

                foreach (var messageType in messageTypes)
                {
                    await _messageBus.SubscribeAsync(messageType.Key, messageType.Value);
                }

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Subscribe Error");
            }

        }
    }
}
