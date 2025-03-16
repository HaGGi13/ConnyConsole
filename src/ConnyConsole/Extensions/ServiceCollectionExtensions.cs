using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Extensions;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddConfiguration(this IServiceCollection services, HostBuilderContext hostContext)
    {
        services.Configure<AppSettings>(hostContext.Configuration.GetSection(AppSettings.SectionName));

        services.AddLogging(builder => builder.AddConsole());

        services.AddTransient<CancellationTokenFactory>();
        services.AddTransient<App>();

        return services;
    }
}
