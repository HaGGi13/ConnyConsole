using ConnyConsole;
using ConnyConsole.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(config =>
        config.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss.fff ";
        }))
    .CreateLogger<Program>();

logger.LogInformation("Starting application...");

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
    logger.LogError(e, "Failed to start application");
    exitCode = -1;
}

logger.LogInformation("Application shutdown.");

return exitCode;
