using System.IO.Abstractions;
using ConnyConsole.Cli;
using ConnyConsole.Cli.Config;
using ConnyConsole.Cli.Log;
using ConnyConsole.Infrastructure;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ConnyConsole.Extensions;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(loggerConfig => loggerConfig.AddDefaultConsoleLogger());

        if (configuration.GetSection("Serilog").Exists())
        {
            services.AddSerilog(loggerConfig =>
                loggerConfig.ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext());
        }

        return services;
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
        services.Configure<CancellationSettings>(configuration.GetSection($"{AppSettings.SectionName}:{CancellationSettings.SectionName}"));

        return services;
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IEnvironmentProvider>(_ => SystemEnvironmentProvider.Instance);

        services.AddKeyedScoped<IConfigurationPathProvider, SystemConfiguration>("System");
        services.AddKeyedScoped<IConfigurationPathProvider, GlobalConfiguration>("Global");
        services.AddKeyedScoped<IConfigurationPathProvider, LocalConfiguration>("Local");

        services.AddScoped<ConsoleCancellationTokenSource>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IConfigurationEditor, JsonConfigurationEditor>();

        services.AddScoped<IApp, App>();

        return services;
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddCliParser(this IServiceCollection services)
    {
        // Arguments
        services.AddScoped<MessageArgument>();
        services.AddScoped<SettingKeyArgument>();
        services.AddScoped<SettingValueArgument>();

        // Options
        services.AddScoped<LocalOption>();
        services.AddScoped<GlobalOption>();
        services.AddScoped<SystemOption>();
        services.AddScoped<CategoryOption>();

        // Commands
        services.AddScoped<LogCommand>();
        services.AddScoped<SetConfigCommand>();
        services.AddScoped<ConfigCommand>();

        services.AddScoped<CliRootCommand>();

        return services;
    }
}
