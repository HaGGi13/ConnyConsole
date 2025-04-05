using System.CommandLine;
using ConnyConsole.Cli.Arguments;
using ConnyConsole.Cli.Options;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Commands;

public class LogCommand : Command
{
    private readonly ILogger<LogCommand> _logger;

    public LogCommand(MessageArgument messageArgument, CategoryOption categoryOption, ILogger<LogCommand> logger)
        : base("log", "Writes a message to the logs.")
    {
        _logger = logger;

        AddAlias("l");

        AddArgument(messageArgument);
        AddOption(categoryOption);

        this.SetHandler(Handle, messageArgument, categoryOption);
    }

    private void Handle(string message, LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Error:
                _logger.LogError(message);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(message);
                break;
            case LogLevel.Trace:
                _logger.LogTrace(message);
                break;
            case LogLevel.Debug:
                _logger.LogDebug(message);
                break;
            case LogLevel.Critical:
                _logger.LogCritical(message);
                break;
            case LogLevel.None:
            case LogLevel.Information:
            default:
                _logger.LogInformation(message);
                break;
        }
    }
}
