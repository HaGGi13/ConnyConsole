using System.CommandLine;

namespace ConnyConsole.Cli.Arguments;

public sealed class MessageArgument() : Argument<string>("message", "The message to write.")
{
}
