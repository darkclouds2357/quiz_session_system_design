using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Diagnostics;

namespace NotifierWorker.Observability
{
    public static class ObservabilityConfiguration
    {
        private static ActivitySource _applicationActivitySource;

        internal static ActivitySource ApplicationActivitySource => _applicationActivitySource;
        private static bool Is(this WriteLogTo writeTo, WriteLogTo compareValue)
        {
            return (writeTo & compareValue) != 0;
        }

        private static LoggerConfiguration WriteToConsole(this LoggerConfiguration loggerConfig, string outputTemplate)
        {
            loggerConfig = !string.IsNullOrWhiteSpace(outputTemplate) ? loggerConfig.WriteTo.Console(outputTemplate: outputTemplate) : loggerConfig.WriteTo.Console(new RenderedCompactJsonFormatter());

            return loggerConfig;
        }

        private static LoggerConfiguration WriteToFile(this LoggerConfiguration loggerConfig, string outputTemplate, string filePath)
        {
            loggerConfig = loggerConfig.WriteTo.File(filePath,
                        outputTemplate: outputTemplate,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        shared: true
                        //, buffered: true
                        );

            return loggerConfig;
        }

        private static LoggerConfiguration WriteToHttp(this LoggerConfiguration loggerConfig, string sinkHttpUrl)
        {
            loggerConfig = loggerConfig.WriteTo.Http(sinkHttpUrl, null);

            return loggerConfig;
        }


        internal static Serilog.ILogger CreateObservabilityLogger(this IConfiguration configuration, ObservabilityOptions observabilityOptions)
        {
            _applicationActivitySource = new ActivitySource(observabilityOptions.ServiceName);

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("AppName", observabilityOptions.ServiceName)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", observabilityOptions.EnviromentName)
                .ReadFrom.Configuration(configuration)
                .Filter.ByExcluding(log =>
                {
                    if (log.Level == LogEventLevel.Error) return false;
                    LogEventPropertyValue name;
                    return log.Properties.TryGetValue("RequestPath", out name) && (name as ScalarValue)?.Value as string == "/ping";
                });
            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

            var outputTemplate = configuration["Serilog:OutputTemplate"];

            if (observabilityOptions.WriteTo.Is(WriteLogTo.Console))
            {
                loggerConfig = loggerConfig.WriteToConsole(outputTemplate);
            }
            if (observabilityOptions.WriteTo.Is(WriteLogTo.File))
            {
                var filePath = configuration["Serilog:FilePath"];
                loggerConfig = loggerConfig.WriteToFile(outputTemplate, filePath);
            }
            if (observabilityOptions.WriteTo.Is(WriteLogTo.Http))
            {
                var sinkHttpUrl = string.IsNullOrWhiteSpace(configuration["Serilog:HttpSyncUrl"]) ? "http://logstash:8080" : configuration["Serilog:HttpSyncUrl"];
                loggerConfig = loggerConfig.WriteToHttp(sinkHttpUrl);
            }
            Log.Logger = loggerConfig.CreateLogger();

            return Log.Logger;
        }

        private static void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args)
        {
            var ex = args.Exception;

            TraceException(ex);
        }

        public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = (Exception)args.ExceptionObject;
            TraceException(ex);
        }

        private static void TraceException(Exception exception)
        {
            var activity = Activity.Current;

            while (activity != null)
            {
                activity.RecordException(exception, default);
                activity.Dispose();
                activity = activity.Parent;
            }
        }
    }
}
