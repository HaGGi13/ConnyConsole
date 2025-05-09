using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class ConfigCommand : Command
{
    public ConfigCommand(SetConfigCommand setConfigCommand) : base("config", "Get or set global or local configuration.")
    {
        AddCommand(setConfigCommand);
    }
}
