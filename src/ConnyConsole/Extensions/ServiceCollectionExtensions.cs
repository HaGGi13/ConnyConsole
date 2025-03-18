using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ConnyConsole.Extensions;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddConfiguration(this IServiceCollection services, HostBuilderContext hostContext)
    {
        services.AddSerilog(loggerConfig =>
            loggerConfig.ReadFrom.Configuration(hostContext.Configuration)
                .Enrich.FromLogContext());

        services.Configure<AppSettings>(hostContext.Configuration.GetSection(AppSettings.SectionName));

        services.AddTransient<ConsoleCancellationTokenSource>();
        services.AddTransient<App>();

        return services;
    }
}
