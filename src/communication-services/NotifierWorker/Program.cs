
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var configuration = Program.GetConfiguration();
var observabilityOptions = new ObservabilityOptions(AppName, EnvironmentName);

Log.Logger = configuration.CreateObservabilityLogger(observabilityOptions);

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    var host = BuildHost(configuration, args);

    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    host.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IHost BuildHost(IConfiguration configuration, string[] args)
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.CaptureStartupErrors(false)
                           .UseKestrel()
                           .UseStartup<Startup>()
                           .UseConfiguration(configuration)
                           .UseContentRoot(Directory.GetCurrentDirectory())
                           .ConfigureKestrel(serverOptions =>
                           {
                           });
             })
            .UseSerilog(Log.Logger);

    return hostBuilder.Build();

}

public partial class Program
{
    public static string AppName = typeof(Startup).Namespace;

    public static string EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    public static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
        return builder.Build();
    }

}
