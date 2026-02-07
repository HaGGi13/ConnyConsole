using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

public sealed partial class LogService
{
    [LoggerMessage(MessageTemplate)]
    private partial void LogMessage(LogLevel logLevel, string message);
}
