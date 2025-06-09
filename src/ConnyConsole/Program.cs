using System.IO.Abstractions;
using ConnyConsole;
using ConnyConsole.Extensions;
using ConnyConsole.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .AddDefaultConsoleLogger("{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

Log.Logger.ForContext<Program>().Debug("Starting application");

int exitCode;
try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostContext, hostConfig) =>
        {
            var env = hostContext.HostingEnvironment;
            var fileSystem = new FileSystem();

            var systemConfiguration = new SystemConfiguration(fileSystem, SystemEnvironmentProvider.Instance);
            hostConfig.AddJsonFile(systemConfiguration.GetConfigFilePath(), optional: true, reloadOnChange: true);

            var globalConfiguration = new GlobalConfiguration(fileSystem, SystemEnvironmentProvider.Instance);
            hostConfig.AddJsonFile(globalConfiguration.GetConfigFilePath(), optional: true, reloadOnChange: true);

            var localConfiguration = new LocalConfiguration(fileSystem);
            hostConfig.AddJsonFile(localConfiguration.GetConfigFilePath(), optional: true, reloadOnChange: true);

            hostConfig.AddJsonFile("Config/appsettings.json", optional: true, reloadOnChange: true);
            hostConfig.AddJsonFile($"Config/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddSerilog(hostContext.Configuration);
            services.AddSettings(hostContext.Configuration);
            services.AddServices();
            services.AddCliParser();
        })
        .Build();

    var app = host.Services.GetRequiredService<IApp>();
    exitCode = await app.RunAsync(args).ConfigureAwait(false);
}
catch (Exception e)
{
    Log.Logger.ForContext<Program>().Error(e, "Failed to start application");
    exitCode = -1;
}
finally
{
    Log.Logger.ForContext<Program>().Debug("Application shutting down...");
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}

return exitCode;
