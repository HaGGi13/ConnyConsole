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
        services.AddTransient<IFileSystem, FileSystem>();
        services.AddTransient<ConsoleCancellationTokenSource>();
        services.AddTransient<ILogService, LogService>();
        services.AddTransient<IConfigurationEditor, JsonConfigurationEditor>();
        services.AddTransient<IApp, App>();

        return services;
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddCliParser(this IServiceCollection services)
    {
        // Arguments
        services.AddTransient<MessageArgument>();
        services.AddTransient<SettingKeyArgument>();
        services.AddTransient<SettingValueArgument>();

        // Options
        services.AddTransient<LocalOption>();
        services.AddTransient<GlobalOption>();
        services.AddTransient<SystemOption>();
        services.AddTransient<CategoryOption>();

        // Commands
        services.AddTransient<LogCommand>();
        services.AddTransient<SetConfigCommand>();
        services.AddTransient<ConfigCommand>();

        services.AddTransient<CliRootCommand>();

        return services;
    }
}
