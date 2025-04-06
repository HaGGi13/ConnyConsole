using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

/// <inheritdoc cref="ILogService{T}"/>
public sealed class LogService<T>(ILogger<T> logger) : ILogService<T> where T : class
{
    public void Log(LogLevel level, string? message)
    {
        var logMessage = message ?? string.Empty;

        switch (level)
        {
            case LogLevel.Critical:
                logger.LogCritical("{logMessage}", logMessage);
                break;
            case LogLevel.Error:
                logger.LogError("{logMessage}", logMessage);
                break;
            case LogLevel.Warning:
                logger.LogWarning("{logMessage}", logMessage);
                break;
            case LogLevel.Debug:
                logger.LogDebug("{logMessage}", logMessage);
                break;
            case LogLevel.Trace:
                logger.LogTrace("{logMessage}", logMessage);
                break;
            case LogLevel.None:
            case LogLevel.Information:
            default:
                logger.LogInformation("{logMessage}", logMessage);
                break;
        }
    }
}
