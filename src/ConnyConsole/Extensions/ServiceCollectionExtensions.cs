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
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSerilog(IConfiguration configuration)
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

        public IServiceCollection AddSettings(IConfiguration configuration)
        {
            services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
            services.Configure<CancellationSettings>(
                configuration.GetSection($"{AppSettings.SectionName}:{CancellationSettings.SectionName}"));

            return services;
        }

        public IServiceCollection AddServices()
        {
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddTransient<IEnvironmentProvider>(_ => SystemEnvironmentProvider.Instance);

            services.AddKeyedTransient<IConfigurationPathProvider, SystemConfiguration>("System");
            services.AddKeyedTransient<IConfigurationPathProvider, GlobalConfiguration>("Global");
            services.AddKeyedTransient<IConfigurationPathProvider, LocalConfiguration>("Local");

            services.AddTransient<ConsoleCancellationTokenSource>();
            services.AddTransient<ILogService, LogService>();
            services.AddTransient<IConfigurationEditor, JsonConfigurationEditor>();

            services.AddTransient<IApp, App>();

            return services;
        }

        public IServiceCollection AddCliParser()
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
}
