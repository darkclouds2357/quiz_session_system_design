using System.Security.Authentication;

namespace NotifierWorker
{
    public class RedisSetting
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string InstanceName { get; set; }
        public string Password { get; set; }
        public bool Ssl { get; set; }
        public SslProtocols? SslProtocols { get; set; }
        public int Database { get; set; } = 0;

        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public int ConnectRetry { get; set; } = 10;
    }
}
