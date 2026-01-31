using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class SettingValueArgument : Argument<string>
{
    public SettingValueArgument() : base("value")
    {
        Description = "The value to set for defined setting key.";
    }
}
