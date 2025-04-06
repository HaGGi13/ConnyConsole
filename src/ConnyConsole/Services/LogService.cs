using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

/// <inheritdoc cref="ILogService{T}"/>
public sealed class LogService<T>(ILogger<T> logger) : ILogService<T> where T : class
{
    private const string MessageTemplate = "{Message}";

    public void Log(LogLevel level, string? message)
    {
        var logMessage = message ?? string.Empty;

        switch (level)
        {
            case LogLevel.Critical:
                logger.LogCritical(MessageTemplate, logMessage);
                break;
            case LogLevel.Error:
                logger.LogError(MessageTemplate, logMessage);
                break;
            case LogLevel.Warning:
                logger.LogWarning(MessageTemplate, logMessage);
                break;
            case LogLevel.Debug:
                logger.LogDebug(MessageTemplate, logMessage);
                break;
            case LogLevel.Trace:
                logger.LogTrace(MessageTemplate, logMessage);
                break;
            default:
                logger.LogInformation(MessageTemplate, logMessage);
                break;
        }
    }
}
