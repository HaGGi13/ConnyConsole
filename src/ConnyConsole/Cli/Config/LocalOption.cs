using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class LocalOption : Option<bool>
{
    public LocalOption()
        : base(aliases: ["-l", "--local"], description: "Set the configuration for current working directory only.")
    {
        AllowMultipleArgumentsPerToken = false;
    }
}
