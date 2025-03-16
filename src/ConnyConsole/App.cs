using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConnyConsole;

public class App
{
    private readonly AppSettings _appSettings;
    private readonly CancellationTokenFactory _cancellationTokenFactory;
    private readonly ILogger<App> _logger;

    public App(IOptions<AppSettings> appSettings, CancellationTokenFactory cancellationTokenFactory, ILogger<App> logger)
    {
        _appSettings = appSettings.Value;
        _cancellationTokenFactory = cancellationTokenFactory;
        _logger = logger;

        RegisterCancellation();
    }

    public Task<int> RunAsync()
    {
        while (!_cancellationTokenFactory.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("I'm working every {LoopOutputInterval} seconds...",
                _appSettings.LoopOutputInterval.TotalSeconds);

            // just some dirty pseudo work
            Thread.Sleep(_appSettings.LoopOutputInterval);
        }

        _logger.LogInformation("Bye bye!");

        return Task.FromResult(0);
    }

    private void RegisterCancellation()
    {
        Console.CancelKeyPress +=
            _cancellationTokenFactory.CreateHandler(_appSettings.CancellationTimeout);
    }
}
