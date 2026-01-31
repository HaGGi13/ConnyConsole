using System.CommandLine;

namespace ConnyConsole.Cli.Log;

public sealed class MessageArgument : Argument<string>
{
    public MessageArgument() : base("message")
    {
        Description = "The message to write.";
    }
}
