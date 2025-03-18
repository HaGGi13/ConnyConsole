// This class function is based on https://medium.com/@sawyer.watts/a-beginners-guide-to-net-s-hostbuilder-part-2-cancellation-857ae3e6ff02

using Microsoft.Extensions.Logging;

namespace ConnyConsole.Infrastructure;

public sealed class CancellationTokenFactory(ILogger<CancellationTokenFactory> logger)
{
    private bool _gracefulCancel = true;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// Creates a <see cref="ConsoleCancelEventHandler"/> for a gracefully (first Ctrl+C) or forced (second Ctrl+C) application exit.
    /// It can be registered on the <see cref="Console.CancelKeyPress"/> event.
    /// </summary>
    /// <param name="timeout">The timeout after which the app is forcibly terminated.</param>
    /// <returns>The configured <see cref="ConsoleCancelEventHandler"/> event.</returns>
    public ConsoleCancelEventHandler CreateHandler(TimeSpan timeout)
    {
        return (_, cancelEvent) =>
        {
            if (_gracefulCancel)
            {
                logger.LogInformation(
                    "Received interrupt signal, attempting to shut down gracefully but will force-close in {Seconds} seconds. Send again to immediately force-close.",timeout.TotalSeconds);

                _cancellationTokenSource.Cancel();
                cancelEvent.Cancel = true;
                _gracefulCancel = false;

                ForceExitAfterTimeout((int)timeout.TotalMilliseconds);
            }
            else
            {
                logger.LogInformation("Second interrupt received, force-closing the app");
            }
        };
    }

    /// <summary>
    /// Waits a defined timeout in milliseconds, afterward enforces the application exit.
    /// </summary>
    private void ForceExitAfterTimeout(int timeoutInMilliseconds)
    {
        _ = new Timer(
            _ =>
            {
                logger.LogInformation("Timeout reached, force-closing app.");
                Environment.Exit(0);
            },
            state: null,
            dueTime: timeoutInMilliseconds,
            period: 0);
    }
}
