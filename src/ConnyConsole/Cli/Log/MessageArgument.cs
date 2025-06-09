using System.CommandLine;

namespace ConnyConsole.Cli.Log;

public sealed class MessageArgument() : Argument<string>("message", "The message to write.")
{
}
