using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

/// <summary>
/// Provides log capabilities.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Logs a message with the defined <see cref="LogLevel" />
    /// </summary>
    /// <param name="level">The message's log level.</param>
    /// <param name="message">The message to log.</param>
    void Log(LogLevel level, string? message);
}
