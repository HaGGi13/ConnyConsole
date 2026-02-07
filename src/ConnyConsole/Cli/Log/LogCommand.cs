using System.CommandLine;
using ConnyConsole.Services;

namespace ConnyConsole.Cli.Log;

public sealed class LogCommand : Command
{
    private readonly CategoryOption _categoryOption;
    private readonly ILogService _logService;
    private readonly MessageArgument _messageArgument;

    public LogCommand(MessageArgument messageArgument, CategoryOption categoryOption, ILogService logService)
        : base("log", "Writes a message to the logs.")
    {
        _messageArgument = messageArgument;
        _categoryOption = categoryOption;
        _logService = logService;

        Aliases.Add("l");

        Arguments.Add(messageArgument);

        Options.Add(categoryOption);

        SetAction(Handle);
    }

    private void Handle(ParseResult parseResult)
    {
        _logService.Log(parseResult.GetValue(_categoryOption), parseResult.GetValue(_messageArgument));
    }
}
