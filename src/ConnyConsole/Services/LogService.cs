using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

/// <inheritdoc cref="ILogService{T}"/>
public sealed class LogService<T>(ILogger<T> logger) : ILogService<T> where T : class
{
    public void Log(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Error:
                logger.LogError(message);
                break;
            case LogLevel.Warning:
                logger.LogWarning(message);
                break;
            case LogLevel.Trace:
                logger.LogTrace(message);
                break;
            case LogLevel.Debug:
                logger.LogDebug(message);
                break;
            case LogLevel.Critical:
                logger.LogCritical(message);
                break;
            case LogLevel.None:
            case LogLevel.Information:
            default:
                logger.LogInformation(message);
                break;
        }
    }
}
