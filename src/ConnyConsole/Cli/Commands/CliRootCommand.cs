using System.CommandLine;

namespace ConnyConsole.Cli.Commands;

public class CliRootCommand : RootCommand
{
    public CliRootCommand(LogCommand logCommand) : base("ConnyConsole - an example implementation")
    {
        AddCommand(logCommand);
    }
}
