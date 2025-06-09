using System.CommandLine;

namespace ConnyConsole.Cli.Config;

public sealed class SettingValueArgument() : Argument<string>("value", "The value to set for defined setting key.");
