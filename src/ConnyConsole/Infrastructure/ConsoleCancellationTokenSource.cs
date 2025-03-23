using Microsoft.Extensions.Logging;

namespace ConnyConsole.Infrastructure;

/// <inheritdoc/>
public class ConsoleCancellationTokenSource(ILogger<ConsoleCancellationTokenSource> logger)
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
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Timeout must be greater or equal zero");
        }

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

                EnforceExitAfterTimeout((int)timeout.TotalMilliseconds);
            }
            else
            {
                logger.LogInformation("Second interrupt received, force-closing the app");
                ExitApplication();
            }
        };
    }

    /// <summary>
    /// Waits a defined timeout in milliseconds, afterward enforces the application exit.
    /// </summary>
    private void EnforceExitAfterTimeout(int timeoutInMilliseconds)
    {
        _ = new Timer(
            _ =>
            {
                logger.LogInformation("Timeout reached, force-closing app.");
                ExitApplication();
            },
            state: null,
            dueTime: timeoutInMilliseconds,
            period: 0);
    }

    /// <summary>
    /// Terminate current process and return exit code 0 to the operating system.
    /// </summary>
    protected virtual void ExitApplication() => Environment.Exit(0);
}
