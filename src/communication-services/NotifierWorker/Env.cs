namespace NotifierWorker
{
    public class Env
    {
        public readonly static bool REDIS_SSL = bool.TryParse(Environment.GetEnvironmentVariable("REDIS_SSL"), out bool isSsl) && isSsl;
        public readonly static int REDIS_DB = int.TryParse(Environment.GetEnvironmentVariable("REDIS_DB"), out int db) ? db : 0;
        public readonly static string REDIS_HOST = Environment.GetEnvironmentVariable("REDIS_HOST");
        public readonly static string REDIS_PASSWORD = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
        public readonly static int REDIS_PORT = int.TryParse(Environment.GetEnvironmentVariable("REDIS_PORT"), out int redisPort) ? redisPort : 6379;
        public readonly static int REDIS_TIMEOUT_RETRY = int.TryParse(Environment.GetEnvironmentVariable("REDIS_TIMEOUT_RETRY"), out int redisRetry) ? redisRetry : 10;

    }
}
