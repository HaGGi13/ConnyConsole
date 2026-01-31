using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class GlobalOption : Option<bool>
{
    public GlobalOption()
        : base("--global", "-g")
    {
        Description = "Set the configuration on user-level globally.";
        AllowMultipleArgumentsPerToken = false;
    }
}
