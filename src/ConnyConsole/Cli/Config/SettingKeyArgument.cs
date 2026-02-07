using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class SettingKeyArgument : Argument<string>
{
    public SettingKeyArgument() : base("key") => Description = "The setting key to set the value for. No wildcards supported.";
}
