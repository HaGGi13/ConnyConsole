using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class LocalOption : Option<bool>
{
    public LocalOption()
        : base("--local", "-l")
    {
        Description = "Set the configuration for current working directory only.";
        AllowMultipleArgumentsPerToken = false;
    }
}
