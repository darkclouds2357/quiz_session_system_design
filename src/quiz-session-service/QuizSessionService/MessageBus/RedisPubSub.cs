

using Newtonsoft.Json;
using QuizSessionService.Application.Commands;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace QuizSessionService.MessageBus
{
    /// <summary>
    /// For this impl use Redis Pub Sub as Message Broker
    /// For real impl should use Kafka rather than RabbitMQ, because with huge streamming data, kafka process better than memory queue as rabbitMQ
    /// </summary>
    public class RedisPubSub : IMessageBus
    {
        private readonly IRedisClientFactory _clientFactory;
        private readonly IServiceProvider _serviceProvider;

        public RedisPubSub(IRedisClientFactory clientFactory, IServiceProvider serviceProvider)
        {
            this._clientFactory = clientFactory;
            this._serviceProvider = serviceProvider;
        }


        public Task PublishAsync(string messageName, object message, CancellationToken cancellationToken = default)
        {
            var multiplexer = _clientFactory.GetDefaultRedisClient().ConnectionPoolManager.GetConnection();

            var subscriber = multiplexer.GetSubscriber();

            return subscriber.PublishAsync(messageName, JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));
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
                    [Const.START_NEW_QUIZ_SESSION_COMMAND] = typeof(StartNewQuizSessionCommand)
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
