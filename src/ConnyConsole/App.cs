using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConnyConsole;

public class App
{
    private readonly AppSettings _appSettings;
    private readonly ConsoleCancellationTokenSource _consoleCancellationTokenSource;
    private readonly ILogger<App> _logger;

    public App(IOptions<AppSettings> appSettings, ConsoleCancellationTokenSource consoleCancellationTokenSource, ILogger<App> logger)
    {
        _appSettings = appSettings.Value;
        _consoleCancellationTokenSource = consoleCancellationTokenSource;
        _logger = logger;

        RegisterConsoleCancellation();
    }

    public Task<int> RunAsync()
    {
        while (!_consoleCancellationTokenSource.Token.IsCancellationRequested)
        {
            _logger.LogInformation("I'm working every {LoopOutputInterval} seconds...",
                _appSettings.LoopOutputInterval.TotalSeconds);

            // just some dirty pseudo work
            Thread.Sleep(_appSettings.LoopOutputInterval);
        }

        _logger.LogInformation("Bye bye!");

        return Task.FromResult(0);
    }

    private void RegisterConsoleCancellation()
    {
        Console.CancelKeyPress +=
            _consoleCancellationTokenSource.CreateCancellationHandler(_appSettings.CancellationTimeout);
    }
}
