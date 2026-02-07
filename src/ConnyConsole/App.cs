using ConnyConsole.Cli;
using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConnyConsole;

public sealed class App : IApp
{
    public const string Name = "ConnyConsole";
    private readonly CancellationSettings _cancellationSettings;
    private readonly ConsoleCancellationTokenSource _consoleCancellationTokenSource;
    private readonly ILogger<App> _logger;
    private readonly CliRootCommand _rootCommand;

    public App(IOptions<CancellationSettings> cancellationSettings, ConsoleCancellationTokenSource consoleCancellationTokenSource,
        CliRootCommand rootCommand, ILogger<App> logger)
    {
        _cancellationSettings = cancellationSettings.Value;
        _consoleCancellationTokenSource = consoleCancellationTokenSource;
        _rootCommand = rootCommand;
        _logger = logger;

        RegisterConsoleCancellation();
    }

    public Task<int> RunAsync(string[] arguments)
    {
        _logger.LogDebug("Start processing commands...");

        return _rootCommand.Parse(arguments).InvokeAsync();
    }

    private void RegisterConsoleCancellation()
    {
        Console.CancelKeyPress +=
            _consoleCancellationTokenSource.CreateCancellationHandler(_cancellationSettings.Timeout);
    }
}
