using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class SystemOption : Option<bool>
{
    public SystemOption() : base("--system", "-s")
    {
        Description = "Set the configuration on system-level for all users.";
        AllowMultipleArgumentsPerToken = false;
    }
}
