using QuizSessionService.Domain.DomainEvents;
using QuizSessionService.Infrastructure.EventStore;
using QuizSessionService.Infrastructure.Query;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis;
using QuizSessionService.Domain;
using Newtonsoft.Json;
using static QuizSessionService.Infrastructure.QuizSessionField;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace QuizSessionService.Infrastructure
{
    /// <summary>
    /// Use same redis as event store and query for simple poc
    /// in real impl event store and query should be in difference way, should not in same
    /// </summary>
    public class RedisDbContext : IEventStore, IQuizSessionQuery
    {


        //private string UserQuizSessionKey(string quizSessionId)
        //{
        //    return $"quiz:session:{quizSessionId}:attended_user:{userId}";
        //}

        private readonly IRedisClientFactory _clientFactory;
        private IDatabase _database => _command.Database;
        private IRedisDatabase _command => _clientFactory.GetRedisDatabase();
        public RedisDbContext(IRedisClientFactory clientFactory)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<LinkedList<UserRank>> GetLeaderboardAsync(string quizSessionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quizSessionKey = QuizSessionField.QuizSessionKey(quizSessionId);
            var leaderboardKey = $"{quizSessionKey}:{QuizSessionField.LEADERBOARD}";

            var leaderboard = await _database.SortedSetRangeByRankWithScoresAsync(leaderboardKey, order: Order.Descending);

            var result = new LinkedList<UserRank>();

            for (int i = 0; i <= leaderboard.Length - 1; i++)
            {
                var rank = leaderboard[i];


                var userRankDetail = rank.Element.ToString().Split(':');
                result.AddLast(new UserRank
                {
                    Rank = i + 1,
                    Score = (int)rank.Score,
                    UserId = userRankDetail[1],
                    UserName = userRankDetail[0]
                });
            }

            return result;

        }
        /// <summary>
        /// because this is sample POC so use redis as event store and save without TTL
        /// WONT USE THIS IN REAL IMPL
        /// </summary>
        /// <param name="streamId"></param>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddEventAsync(string streamId, IDomainEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var quizSessionKey = QuizSessionField.QuizSessionKey(@event.StreamId);
            var key = $"{quizSessionKey}:{QuizSessionField.EVENT_STORE}";

            var typeName = @event.GetType().FullName;
            var payload = JsonConvert.SerializeObject(@event);

            var eventData = new Dictionary<string, string>
            {
                [EventStoreField.STREAM_ID] = @event.StreamId,
                [EventStoreField.VERSION] = @event.Version.ToString(),
                [EventStoreField.CREATED_AT] = @event.CreatedAt.ToString(),
                [EventStoreField.EVENT_NAME] = @event.EventName,
                [EventStoreField.EVENT_ASSEMBLY_TYPE] = typeName,
                [EventStoreField.PAYLOAD] = payload,
            };
            var eventDataJson = JsonConvert.SerializeObject(eventData);

            await _database.SortedSetAddAsync(key, eventDataJson, @event.Version);

        }

        /// <summary>
        /// because this is sample POC so use redis as event store and save without TTL
        /// WONT USE THIS IN REAL IMPL
        /// </summary>
        /// <param name="streamId"></param>
        /// <param name="fromVersion"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(string streamId, int fromVersion = 0, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = $"{QuizSessionField.QuizSessionKey(streamId)}:{QuizSessionField.EVENT_STORE}";

            var events = await _database.SortedSetRangeByScoreAsync(key, fromVersion);
            var result = new List<IDomainEvent>();

            for (int i = 0; i < events.Length; i++)
            {
                var eventData = JsonConvert.DeserializeObject<Dictionary<string, string>>(events[i]);
                var eventTypeName = eventData[EventStoreField.EVENT_ASSEMBLY_TYPE];
                var payload = eventData[EventStoreField.PAYLOAD];
                var eventType = Type.GetType(eventTypeName);
                if (eventType == null)
                    continue;

                var @event = (IDomainEvent)JsonConvert.DeserializeObject(payload, eventType);
                if (@event == null)
                    continue;

                result.Add(@event);
            }

            return result;
        }



        public Task ApplyQueryEventAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var retryCount = 0;

            while (retryCount < Env.REDIS_TIMEOUT_RETRY)
            {
                try
                {
                    return (Task)((dynamic)this).ApplyAsync((dynamic)@event, cancellationToken);
                }
                catch (RedisTimeoutException)
                {
                    retryCount++;
                }
                catch
                {
                    throw;
                }
            }
            return Task.CompletedTask;

        }


        public async Task ApplyAsync(QuizSessionStartedEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = $"{QuizSessionField.QuizSessionKey(@event.StreamId)}:snap_shot";

            var hash = new[]
            {
                new HashEntry(QuizSessionField.ID, @event.StreamId),
                new HashEntry(QuizSessionField.VERSION, @event.Version),
                new HashEntry(QuizSessionField.START_TIME, @event.CreatedAt.ToString()),
                new HashEntry(QuizSessionField.END_TIME, @event.EndTime.ToString()),
                new HashEntry(QuizSessionField.QUESTIONS, JsonConvert.SerializeObject(@event.SessionQuestions))
            };

            await _database.HashSetAsync(key, hash, CommandFlags.PreferMaster);
            await _database.KeyExpireAsync(key, @event.EndTime, CommandFlags.PreferMaster);

        }

        public async Task ApplyAsync(UserAttendedQuizSessionEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quizSessionKey = QuizSessionField.QuizSessionKey(@event.StreamId);

            await _database.HashSetAsync($"{quizSessionKey}:snap_shot", QuizSessionField.VERSION, @event.Version, flags: CommandFlags.PreferMaster);

            var key = $"{quizSessionKey}:{QuizSessionField.ATTENDED_USERS}:{@event.UserId}";

            var hash = new[]
            {
                new HashEntry(AttendedUserField.ID, @event.UserId),
                new HashEntry(AttendedUserField.USER_NAME, @event.UserName),
                new HashEntry(AttendedUserField.CURRENT_EXAM, JsonConvert.SerializeObject(@event.NextQuestion)),
            };

            await _database.HashSetAsync(key, hash, CommandFlags.PreferMaster);


            // set TTL as the time of end quiz session time
            var endTimeString = await _database.HashGetAsync($"{quizSessionKey}:snap_shot", QuizSessionField.END_TIME, CommandFlags.PreferReplica);
            if (DateTime.TryParse(endTimeString, out var endTime))
            {
                await _database.KeyExpireAsync(key, endTime.Subtract(DateTime.UtcNow), CommandFlags.PreferMaster);
            }

        }

        public async Task ApplyAsync(LeaderboardRankUpdatedEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var quizSessionKey = QuizSessionField.QuizSessionKey(@event.StreamId);

            // Update leaderboard
            var leaderboardKey = $"{quizSessionKey}:{QuizSessionField.LEADERBOARD}";

            foreach (var item in @event.Leaderboard)
            {
                await _database.SortedSetAddAsync(leaderboardKey, QuizSessionField.LeaderboardMember(item.UserId, item.UserName), item.Score, CommandFlags.PreferMaster);

                // Set ttl for new key
                if (await _database.KeyTimeToLiveAsync(leaderboardKey) == null)
                {
                    var endTimeString = await _database.HashGetAsync($"{quizSessionKey}:snap_shot", QuizSessionField.END_TIME, CommandFlags.PreferReplica);
                    if (DateTime.TryParse(endTimeString, out var endTime))
                    {
                        await _database.KeyExpireAsync(leaderboardKey, endTime.Subtract(DateTime.UtcNow), CommandFlags.PreferMaster);
                    }
                }

            }

        }
        public async Task ApplyAsync(UserSubmittedAnswerEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quizSessionKey = QuizSessionField.QuizSessionKey(@event.StreamId);
            await _database.HashSetAsync($"{quizSessionKey}:snap_shot", QuizSessionField.VERSION, @event.Version, flags: CommandFlags.PreferMaster);
            var key = $"{quizSessionKey}:{QuizSessionField.ATTENDED_USERS}:{@event.UserId}";

            // snapshot next question
            await _database.HashSetAsync(key, AttendedUserField.CURRENT_EXAM, JsonConvert.SerializeObject(@event.NextQuestion), flags: CommandFlags.PreferMaster);


            // update answered question
            var answeredQuestionKey = $"{key}:{AttendedUserField.ANSWERED_QUESTIONS}";

            await _database.SetAddAsync(answeredQuestionKey, AttendedUserField.AnsweredQuestionMember(@event.SessionQuestionId, @event.AnsweredIds, @event.IsCorrect, @event.QuestionScore), CommandFlags.PreferMaster);

            // Set ttl for new key
            if (await _database.KeyTimeToLiveAsync(answeredQuestionKey) == null)
            {
                var endTimeString = await _database.HashGetAsync($"{quizSessionKey}:snap_shot", QuizSessionField.END_TIME, CommandFlags.PreferReplica);
                if (DateTime.TryParse(endTimeString, out var endTime))
                {
                    await _database.KeyExpireAsync(answeredQuestionKey, endTime.Subtract(DateTime.UtcNow), CommandFlags.PreferMaster);
                }
            }

        }


        public async Task ApplyAsync(UserRankChangedEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var quizSessionKey = QuizSessionField.QuizSessionKey(@event.StreamId);

            await _database.HashSetAsync($"{quizSessionKey}:snap_shot", QuizSessionField.VERSION, @event.Version, flags: CommandFlags.PreferMaster);

            var key = $"{quizSessionKey}:{QuizSessionField.ATTENDED_USERS}:{@event.UserId}";

            var hash = new[]
            {
                new HashEntry(AttendedUserField.RANK, @event.NewRank),
                new HashEntry(AttendedUserField.PREVIOUS_RANK, @event.PreviousRank),
                new HashEntry(AttendedUserField.SCORE, @event.Score),
            };

            await _database.HashSetAsync(key, hash, CommandFlags.PreferMaster);

        }

    }
}
