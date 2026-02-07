using Microsoft.Extensions.Logging;

namespace ConnyConsole.Infrastructure;

public sealed partial class ConsoleCancellationTokenSource
{
    [LoggerMessage(LogLevel.Information,
        "Received interrupt signal, attempting to shut down gracefully but will force-close in {seconds} seconds. Send again to immediately force-close.")]
    private partial void LogGracefulShutdownInitiated(double seconds);

    [LoggerMessage(LogLevel.Information, "Second interrupt received, force-closing the app")]
    private partial void LogForceShutdownInitiated();

    [LoggerMessage(LogLevel.Information, "Timeout reached, force-closing app.")]
    private partial void LogForceShutdownAfterTimeout();
}
