using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

/// <inheritdoc cref="ILogService" />
public sealed class LogService(ILogger<LogService> logger) : ILogService
{
    private const string MessageTemplate = "{Message}";

    public void Log(LogLevel level, string? message)
    {
        var logMessage = message ?? string.Empty;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault - is marked by SonarQube rule csharpsquid:S3458
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
