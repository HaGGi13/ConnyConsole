using System.CommandLine;
using ConnyConsole.Cli.Config;
using ConnyConsole.Cli.Log;

namespace ConnyConsole.Cli;

public sealed class CliRootCommand : RootCommand
{
    public CliRootCommand(LogCommand logCommand, ConfigCommand configCommand) : base("ConnyConsole - an example implementation")
    {
        Subcommands.Add(logCommand);
        Subcommands.Add(configCommand);
    }
}
