using ConnyConsole;
using ConnyConsole.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Display;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new MessageTemplateTextFormatter(
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} {Message:lj}{NewLine}{Exception}"))
    .CreateBootstrapLogger();

Log.Logger.ForContext<Program>().Information("Starting application");

int exitCode;
try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostContext, hostConfig) =>
        {
            var env = hostContext.HostingEnvironment;

            hostConfig.AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true);
            hostConfig.AddJsonFile($"Config/appsettings.{env.EnvironmentName}.json", optional: true,
                reloadOnChange: true);
        })
        .ConfigureServices((hostContext, services) => services.AddConfiguration(hostContext))
        .Build();

    var app = host.Services.GetRequiredService<App>();
    exitCode = await app.RunAsync().ConfigureAwait(false);
}
catch (Exception e)
{
    Log.Logger.ForContext<Program>().Error(e, "Failed to start application");
    exitCode = -1;
}
finally
{
    Log.Logger.ForContext<Program>().Information("Application shutting down...");
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}

return exitCode;
