using System.CommandLine;

namespace ConnyConsole.Cli.Arguments;

public class MessageArgument() : Argument<string>("message", "The message to write.")
{
}
