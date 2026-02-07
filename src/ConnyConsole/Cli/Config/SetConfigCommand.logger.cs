using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Config;

public sealed partial class SetConfigCommand
{
    [LoggerMessage(LogLevel.Information, "Set setting result: {message}")]
    private partial void LogSetSettingResultMessage(string message);
}
