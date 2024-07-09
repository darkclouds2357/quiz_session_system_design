namespace QuizSessionService.Observability
{
    public class ObservabilityOptions
    {
        private static readonly string _writeToValue = Environment.GetEnvironmentVariable("WRITE_LOG_TO");

        public ObservabilityOptions(string serviceName, string enviromentName)
        {
            ServiceName = serviceName;
            EnviromentName = enviromentName;
        }

        public string ServiceName { get; }
        public string EnviromentName { get; }

        public WriteLogTo WriteTo
        {
            get
            {
                if (Enum.TryParse(typeof(WriteLogTo), _writeToValue, out var result) && result is WriteLogTo writeTo)
                {
                    return writeTo;
                }
                return WriteLogTo.Console;
            }
        }


    }

    public enum WriteLogTo
    {
        //None = 0,
        Console = 1,
        File = 2,
        Http = 4,
        //OTel = 8,
    }
}
