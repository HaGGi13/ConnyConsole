using Microsoft.Extensions.Logging;

namespace ConnyConsole.Infrastructure;

/// <inheritdoc/>
public sealed class ConsoleCancellationTokenSource(ILogger<ConsoleCancellationTokenSource> logger)
    : CancellationTokenSource
{
    private bool _isGracefulCancelled = true;

    /// <summary>
    /// Creates a <see cref="ConsoleCancelEventHandler"/> for a gracefully (first Ctrl+C) or forced (second Ctrl+C) application exit.
    /// It can be registered on the <see cref="Console.CancelKeyPress"/> event.
    /// </summary>
    /// <param name="timeout">The timeout after which the app is forcibly terminated.</param>
    /// <returns>The configured <see cref="ConsoleCancelEventHandler"/> event.</returns>
    /// <remarks>This method is based on https://medium.com/@sawyer.watts/a-beginners-guide-to-net-s-hostbuilder-part-2-cancellation-857ae3e6ff02</remarks>
    public ConsoleCancelEventHandler CreateCancellationHandler(TimeSpan timeout)
    {
        return (_, cancelEvent) =>
        {
            if (_isGracefulCancelled)
            {
                logger.LogInformation(
                    "Received interrupt signal, attempting to shut down gracefully but will force-close in {Seconds} seconds. Send again to immediately force-close.",
                    timeout.TotalSeconds);

                Cancel();
                cancelEvent.Cancel = true;
                _isGracefulCancelled = false;

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
