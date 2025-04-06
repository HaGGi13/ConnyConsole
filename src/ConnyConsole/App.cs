using System.CommandLine;
using ConnyConsole.Cli.Commands;
using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConnyConsole;

public sealed class App : IApp
{
    private readonly AppSettings _appSettings;
    private readonly ConsoleCancellationTokenSource _consoleCancellationTokenSource;
    private readonly CliRootCommand _rootCommand;
    private readonly ILogger<App> _logger;

    public App(IOptions<AppSettings> appSettings, ConsoleCancellationTokenSource consoleCancellationTokenSource,
         CliRootCommand rootCommand, ILogger<App> logger)
    {
        _appSettings = appSettings.Value;
        _consoleCancellationTokenSource = consoleCancellationTokenSource;
        _rootCommand = rootCommand;
        _logger = logger;

        RegisterConsoleCancellation();
    }

    public Task<int> RunAsync(string[] arguments)
    {
        _logger.LogDebug("Start processing commands...");

        return _rootCommand.InvokeAsync(arguments);
    }

    private void RegisterConsoleCancellation()
    {
        Console.CancelKeyPress +=
            _consoleCancellationTokenSource.CreateCancellationHandler(_appSettings.CancellationTimeout);
    }
}
