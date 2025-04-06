using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

/// <summary>
/// Provides log capabilities.
/// </summary>
/// <typeparam name="T">Type of class that uses the log service.</typeparam>
public interface ILogService<T> where T : class
{
    /// <summary>
    /// Logs a message with the defined <see cref="LogLevel"/>
    /// </summary>
    /// <param name="level">The message's log level.</param>
    /// <param name="message">The message to log.</param>
    void Log(LogLevel level, string message);
}
