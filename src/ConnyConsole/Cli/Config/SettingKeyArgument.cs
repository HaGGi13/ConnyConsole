using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class SettingKeyArgument() : Argument<string>("key", "The setting key to set the value for. No wildcards supported.");
