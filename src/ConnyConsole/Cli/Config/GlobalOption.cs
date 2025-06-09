using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class GlobalOption : Option<bool>
{
    public GlobalOption()
        : base(aliases: ["-g", "--global"], "Set the configuration on user-level globally.")
    {
        AllowMultipleArgumentsPerToken = false;
    }
}
