using System.CommandLine;
using ConnyConsole.Services;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Log;

public sealed class LogCommand : Command
{
    private readonly ILogService _logService;

    public LogCommand(MessageArgument messageArgument, CategoryOption categoryOption, ILogService logService)
        : base("log", "Writes a message to the logs.")
    {
        _logService = logService;

        AddAlias("l");

        AddArgument(messageArgument);
        AddOption(categoryOption);

        this.SetHandler(Handle, categoryOption, messageArgument);
    }

    private void Handle(LogLevel level, string message)
    {
        _logService.Log(level, message);
    }
}
