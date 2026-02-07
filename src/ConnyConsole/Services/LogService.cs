using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

/// <inheritdoc cref="ILogService" />
public sealed partial class LogService(ILogger<LogService> logger) : ILogService
{
    private const string MessageTemplate = "{message}";

    public void Log(LogLevel level, string? message)
    {
        var logMessage = message ?? string.Empty;

        var logLevel = level == LogLevel.None
            ? LogLevel.Information
            : level;

        LogMessage(logLevel, logMessage);
    }
}
